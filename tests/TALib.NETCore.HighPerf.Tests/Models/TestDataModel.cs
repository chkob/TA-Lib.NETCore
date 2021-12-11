using System;

namespace TALib.NETCore.HighPerf.Tests.Models
{
    public class TestDataModel
    {
        public string Name { get; set; } = null!;

        public double[][] Inputs { get; set; } = null!;

        public double[] Options { get; set; } = Array.Empty<double>();

        public double[][] Outputs { get; set; } = null!;

        public bool Skip { get; set; }

        public override string ToString() => Name;
    }
}
