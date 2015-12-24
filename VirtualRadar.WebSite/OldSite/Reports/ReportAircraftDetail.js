function ReportAircraftDetail()
{
    var that = this;
    this.registerSubclass = function(value) { that = value; return value; };
    var mReportDescription = null;
    var mShowLinksToAircraftReports = false;
    this.setShowLinksToAircraftReports = function(value) { mShowLinksToAircraftReports = value; };

    this.clear = function()
    {
        document.getElementById('aircraftDetail').innerHTML = '';
    };

    this.showDetail = function(aircraft)
    {
        var html = '';

        if(aircraft.isUnknown) html += 'Registration not recognised';
        else {
            var reg = aircraft.reg;
            var icao = aircraft.icao;
            if(mShowLinksToAircraftReports) {
                reg = formatRegReportLink(reg);
                icao = formatIcaoReportLink(icao);
            }

            // Detail heading table
            html += '<table class="detailHeadingTable">' +
                    '<tr>' +
                    '<td class="detailHeading detailRegistration">' + reg + '</td>' +
                    '<td class="detailHeading detailIcao">' + icao + '</td>' +
                    '<td class="detailOpFlag">';
            html += formatOperatorFlag(aircraft);
            html += '</td>' +
                    '</tr>' +
                    '</table>';

            // Detail summary table
            html +=
                '<table class="detailSummaryTable">' +
                '<tr><td class="detailHeading detailOperator">' +
                aircraft.owner +
                '</td><td class="detailHeading detailOpFlagCode">' +
                aircraft.opFlag +
                '</td></tr>' +
                '<tr><td class="detailHeading detailModel">' +
                (aircraft.typ != '' ? aircraft.typ : aircraft.popularName) +
                '</td><td class="detailHeading detailType">' +
                aircraft.icaoType +
                '</td></tr>' +
                '</table>';

            // Other information table
            html += '<table class="detailOtherTable">';

            if(that.subclassAddToStartOfOtherTable !== undefined) html += that.subclassAddToStartOfOtherTable();

            if(aircraft.status !== undefined) html += that.addOtherRow('Status', aircraft.status);
            if(aircraft.previousId !== undefined) html += that.addOtherRow('Previous ID', aircraft.previousId);
            if(aircraft.country !== undefined) html += that.addOtherRow('Country', aircraft.country);
            if(aircraft.modeSCountry !== '') html += that.addOtherRow('Mode-S Country', aircraft.modeSCountry);
            html += that.addOtherRow('Military', aircraft.military ? 'Yes' : 'No');
            if(aircraft.firstRegDate !== undefined) html += that.addOtherRow('First Reg. Date', aircraft.firstRegDate);
            if(aircraft.curRegDate !== undefined) html += that.addOtherRow('Current Reg. Date', aircraft.curRegDate);
            if(aircraft.deregDate !== undefined) html += that.addOtherRow('Dereg. Date', aircraft.deregDate);
            if(aircraft.cofACategory !== undefined) html += that.addOtherRow('C.o.A. Category', aircraft.cofACategory);
            if(aircraft.cofAExpiry !== undefined) html += that.addOtherRow('C.o.A. Expires', aircraft.cofAExpiry);

            if(aircraft.acClass !== undefined) html += that.addOtherRow('Aircraft Class', aircraft.acClass);
            if(aircraft.manufacturer !== undefined) html += that.addOtherRow('Manufacturer', aircraft.manufacturer);
            if(aircraft.genericName !== undefined) html += that.addOtherRow('Generic Name', aircraft.genericName);
            if(aircraft.popularName !== '') html += that.addOtherRow('Popular Name', aircraft.popularName);
            if(aircraft.engines) html += that.addOtherRow('Engines', capitaliseSentence(formatEngines(aircraft.engines, aircraft.engType)));
            if(aircraft.wtc) html += that.addOtherRow('Weight', capitaliseSentence(formatWakeTurbulenceCategory(aircraft.wtc, false, true)));
            if(aircraft.species) html += that.addOtherRow('Species', capitaliseSentence(formatSpecies(aircraft.species, false)));
            if(aircraft.mtow !== undefined) html += that.addOtherRow('Max. Takeoff Weight', aircraft.mtow);
            if(aircraft.totalHours !== undefined) html += that.addOtherRow('Total Hours', aircraft.totalHours);
            if(aircraft.yearBuilt !== undefined) html += that.addOtherRow('Year Built', aircraft.yearBuilt);
            if(aircraft.serial !== undefined) html += that.addOtherRow('Serial', aircraft.serial);

            if(aircraft.infoUrl !== undefined) html += that.addOtherRow('More Information', aircraft.infoUrl);
            if(aircraft.interested !== undefined && aircraft.interested) html += that.addOtherRow('Interested', 'Yes');
            if(aircraft.pictureUrl1 !== undefined) html += that.addOtherRow('Picture 1', aircraft.pictureUrl1);
            if(aircraft.pictureUrl2 !== undefined) html += that.addOtherRow('Picture 1', aircraft.pictureUrl2);
            if(aircraft.pictureUrl3 !== undefined) html += that.addOtherRow('Picture 1', aircraft.pictureUrl3);

            if(aircraft.notes !== undefined) html += that.addOtherRow('Notes', aircraft.notes);

            if(that.subclassAddToEndOfOtherTable !== undefined) html += that.subclassAddToEndOfOtherTable();

            html += "</table>";

            if(aircraft.hasPic) {
                html += '<div class="detailPicture">' + formatPicture(aircraft, null, 'detailPictureImg', true, 'detail') + '</div>';
            }

            if(aircraft.reg) {
                var afr = formatAirframesOrgRegistration(aircraft.reg);
                html += '<div class="detailLinkList">';
                html += '<a href="http://www.airliners.net/search/photo.search?regsearch=' + aircraft.reg + '" target="_airlinersnet">www.airliners.net</a>';
                if(afr) html += ' :: <a href="http://www.airframes.org/reg/' + afr + '" target="_airframesorg">www.airframes.org</a>';
                html += '</div>';
            }
        }

        document.getElementById("aircraftDetail").innerHTML = html;
    };

    this.addOtherRow = function(heading, value)
    {
        var result = '';
        if(value !== undefined) {
            if(heading.length > 0) heading += ':';
            result = '<tr><td class="detailOtherHead">' + heading + '</td><td class="detailOtherDetail">' + value + '</td></tr>';
        }

        return result;
    };
}