﻿using Common.Constants.Units;

namespace Common.Services
{
    public interface IMassConverter
    {
        public double Convert(double fromValue, MassUnit fromUnit, MassUnit toUnit);
    }
}
