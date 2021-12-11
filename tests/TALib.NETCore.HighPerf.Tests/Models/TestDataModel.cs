using System;

namespace TALib.NETCore.HighPerf.Tests.Models
{
    public class TestDataModel
    {
        public string Name { get; set; } = null!;

        public decimal[][] Inputs { get; set; } = null!;

        public decimal[] Options { get; set; } = Array.Empty<decimal>();

        public decimal[][] Outputs { get; set; } = null!;

        public bool Skip { get; set; }

        public override string ToString() => Name;
    }
}
