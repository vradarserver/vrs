using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdTree.Math
{
	[Serializable]
	public class GeoMath : FloatMath
	{
		public override float DistanceSquaredBetweenPoints(float[] a, float[] b)
		{
			double dst = GeoUtils.Distance(a[0], a[1], b[0], b[1], 'K');
			return (float)(dst * dst);
		}
	}
}
