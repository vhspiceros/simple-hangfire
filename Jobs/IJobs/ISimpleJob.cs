using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace simple_hangfire.Jobs.IJobs {
    public interface ISimpleJob {
        void Execute();
    }
}
