using System;
using System.Collections.Generic;
using System.Text;

namespace cloudeventsformatdemo
{
    [AttributeUsage(AttributeTargets.Property)]
    class DontCopyAttribute  : Attribute          
    {
    }
}
