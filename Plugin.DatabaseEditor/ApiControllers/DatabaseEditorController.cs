using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Plugin.DatabaseEditor.Models;

namespace VirtualRadar.Plugin.DatabaseEditor.ApiControllers
{
    [Authorize(Roles = Roles.Admin)]
    public class DatabaseEditorController : PipelineApiController
    {
        [HttpGet]
        [Route("DatabaseEditor/SingleAircraftSearch.json")]         // Historical route
        [Route("api/3.00/DatabaseEditor/{icao}")]
        public SingleSearchResultsJson Get(string icao)
        {
            var result = new SingleSearchResultsJson();
            var plugin = Plugin.Singleton;

            if(!String.IsNullOrEmpty(icao)) {
                try {
                    result.Aircraft = plugin.BaseStationDatabase.GetAircraftByCode(icao);
                    plugin.IncrementSearchCount();
                    plugin.UpdateStatusTotals();
                } catch(Exception ex) {
                    result.Exception = plugin.LogException(ex, "Exception caught during DatabaseEditor SingleAircraftSearch ({0}): {1}", icao, ex.ToString());
                }
            }

            return result;
        }

        [HttpPost]
        [Route("DatabaseEditor/SingleAircraftSave.json")]           // Historical route
        [Route("api/1.00/DatabaseEditor")]
        public SingleAircraftSaveResultsJson Save([FromBody] BaseStationAircraft aircraft)
        {
            var result = new SingleAircraftSaveResultsJson();
            var plugin = Plugin.Singleton;

            try {
                result.Aircraft = aircraft;

                if(CustomConvert.Icao24(aircraft?.ModeS) > 0) {
                    aircraft.Registration =     aircraft?.Registration.ToUpper().Trim();
                    aircraft.ICAOTypeCode =     aircraft?.ICAOTypeCode.ToUpper().Trim();
                    aircraft.OperatorFlagCode = aircraft?.OperatorFlagCode.ToUpper().Trim();
                    aircraft.LastModified = DateTime.Now;

                    if(aircraft.UserString1 == "Missing") {
                        aircraft.UserString1 = null;
                    }

                    if(aircraft.AircraftID == 0) {
                        plugin.BaseStationDatabase.InsertAircraft(aircraft);
                    } else {
                        plugin.BaseStationDatabase.UpdateAircraft(aircraft);
                    }

                    plugin.IncrementUpdateCount();
                    plugin.UpdateStatusTotals();
                }
            } catch(Exception ex) {
                var aircraftID = aircraft == null ? "<no aircraft>" : aircraft.AircraftID.ToString();
                var icao = aircraft == null ? "<no aircraft>" : aircraft.ModeS;
                result.Exception = plugin.LogException(ex, "Exception caught during DatabaseEditor SingleAircraftSave ({0}/{1}): {2}", aircraftID, icao, ex.ToString());
            }

            return result;
        }
    }
}
