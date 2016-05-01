using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Plugin.CustomContent
{
    static class Describe
    {
        public static string InjectionLocation(InjectionLocation location)
        {
            switch(location) {
                case CustomContent.InjectionLocation.Body:  return CustomContentStrings.Body;
                case CustomContent.InjectionLocation.Head:  return CustomContentStrings.Head;
                default:                                    throw new NotImplementedException();
            }
        }
    }
}
