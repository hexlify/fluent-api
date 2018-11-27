﻿using System;
using System.Reflection;

namespace ObjectPrinting.Config
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        private PropertyInfo propertyToChange;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyToChange)
        {
            ParentConfig = printingConfig;
            this.propertyToChange = propertyToChange;
        }

        public PrintingConfig<TOwner> Using(Func<object, string> print)
        {
            ParentConfig.OverridePropertyPrinting(propertyToChange, print);
            return ParentConfig;
        }
    }

    public interface IPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}