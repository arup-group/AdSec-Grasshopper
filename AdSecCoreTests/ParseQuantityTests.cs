﻿using AdSecCore;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests {
  public class ParseQuantityTests {
    private readonly LengthUnit units = LengthUnit.Meter;

    [Fact]
    public void ShouldParseDouble() {
      Assert.Equal(new Length(2, units), UnitHelpers.ParseToQuantity<Length>(2.0, units));
    }

    [Fact]
    public void ShouldParseInt() {
      Assert.Equal(new Length(2, units), UnitHelpers.ParseToQuantity<Length>(2, units));
    }

    [Fact]
    public void ShouldParseString() {
      Assert.Equal(new Length(2, units), UnitHelpers.ParseToQuantity<Length>("2", units));
    }

    [Fact]
    public void ShouldParseStringWithUnits() {
      Assert.Equal(new Length(2, units), UnitHelpers.ParseToQuantity<Length>("2m", units));
    }

    [Fact]
    public void ShouldWorkWithOtherUnits() {
      Assert.Equal(new Length(2, LengthUnit.Millimeter),
        UnitHelpers.ParseToQuantity<Length>("2mm", LengthUnit.Millimeter));
    }

    [Fact]
    public void ShouldWorkForOtherQuantities() {
      Assert.Equal(new Area(2, AreaUnit.SquareMeter), UnitHelpers.ParseToQuantity<Area>("2m²", AreaUnit.SquareMeter));
    }
  }
}