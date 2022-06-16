using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using simple_hangfire.Jobs.IJobs;

namespace simple_hangfire.Jobs.Jobs {
    public class SimpleJob : ISimpleJob {
        private readonly ILogger<SimpleJob> _Logger;
        public SimpleJob(ILogger<SimpleJob> logger) {
            _Logger = logger;
        }

        public void Execute() {
            _Logger.LogInformation("this is a simple TEST");
        }
    }
}
