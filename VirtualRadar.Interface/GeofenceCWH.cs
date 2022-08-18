namespace VirtualRadar.Interface
{
    /// <summary>
    /// A geofence that is defined as a Centre point, a Width and a Height.
    /// It isn't a true rectangle but a set of most-westerly, most-northerly,
    /// most-easterly and most-southerly coordinates are calculated for it
    /// and all locations must pass between a rectangle defined by these
    /// points to be included.
    /// </summary>
    public class GeofenceCWH
    {
        /// <summary>
        /// Gets the latitude of the centre point of the fence.
        /// </summary>
        public double CentreLatitude { get; }

        /// <summary>
        /// Gets the longitude of the centre point of the fence.
        /// </summary>
        public double CentreLongitude { get; }

        /// <summary>
        /// Gets the width of the fence in <see cref="DistanceUnit"/> units.
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Gets the height of the fence in <see cref="DistanceUnit"/> units.
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Gets the unit of distance for both <see cref="Width"/> and <see cref="Height"/>.
        /// </summary>
        public DistanceUnit DistanceUnit { get; }

        /// <summary>
        /// Gets the westerly-most longitude of the fence.
        /// </summary>
        public double WesterlyLongitude { get; }

        /// <summary>
        /// Gets the easterly-most longitude of the fence.
        /// </summary>
        public double EasterlyLongitude { get; }

        /// <summary>
        /// Gets the northerly-most latitude of the fence.
        /// </summary>
        public double NortherlyLatitude { get; }

        /// <summary>
        /// Gets the southerly-most latitude of the fence.
        /// </summary>
        public double SoutherlyLatitude { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="centreLatitude"></param>
        /// <param name="centreLongitude"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="distanceUnit"></param>
        public GeofenceCWH(
            double centreLatitude,
            double centreLongitude,
            double width,
            double height,
            DistanceUnit distanceUnit
        )
        {
            CentreLatitude =    centreLatitude;
            CentreLongitude =   centreLongitude;
            Width =             width;
            Height =            height;
            DistanceUnit =      distanceUnit;

            WesterlyLongitude = CalculateBoundary(270);
            NortherlyLatitude = CalculateBoundary(0);
            EasterlyLongitude = CalculateBoundary(90);
            SoutherlyLatitude = CalculateBoundary(180);
        }

        private double CalculateBoundary(double angle)
        {
            var isLat = angle == 0.0 || angle == 180.0;

            GreatCircleMaths.Destination(
                CentreLatitude,
                CentreLongitude,
                angle,
                CustomConvert.DistanceUnits((isLat ? Height : Width) / 2.0, DistanceUnit, DistanceUnit.Kilometres),
                out double? lat,
                out double? lng
            );

            return (isLat ? lat : lng) ?? 0.0;
        }

        /// <summary>
        /// Returns true if the point passed across is within the fence's boundaries.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public bool IsWithinBounds(double? latitude, double? longitude)
        {
            return GeofenceChecker.IsWithinBounds(
                latitude,
                longitude,
                NortherlyLatitude,
                WesterlyLongitude,
                SoutherlyLatitude,
                EasterlyLongitude
            );
        }
    }
}
