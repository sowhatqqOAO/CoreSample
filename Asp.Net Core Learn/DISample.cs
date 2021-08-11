using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.Net_Core_Learn
{
    public interface ISample
    {
        int Id { get; }
    }

    public interface ISampleTransient : ISample
    {
    }

    public interface ISampleScoped : ISample
    {
    }

    public interface ISampleSingleton : ISample
    {
    }

    public class DISample : ISampleTransient, ISampleScoped, ISampleSingleton
    {
        private static int _counter;
        private int _id;

        public DISample()
        {
            _id = ++_counter;
        }

        public int Id => _id;
    }
}
