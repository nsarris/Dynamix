﻿namespace Dynamix.DynamicProjection
{
    internal class CtorParamTargetConfiguration
    {
        public string ParameterName { get; }
        public ProjectionSource Source { get; set; }
        public ValueMap ValueMap { get; set; }
        public CtorParamTargetConfiguration(string parameterName)
        {
            ParameterName = parameterName;
        }
    }
}
