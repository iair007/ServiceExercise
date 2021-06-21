using ConnectionPool;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceExercise
{
    public class Service : IService
    {
        private readonly ILogger<Service> _logger;
        int _connectionCount;
        int _result = 0;
        private object _lock = new object();
        Connection _con;
        Task _mainBackgroundTask;
        BlockingCollection<Request> _queue;

        public Service(int connectionCount)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = serviceProvider.GetService<ILogger<Service>>();

            _logger.LogInformation($"Service(START[{connectionCount}])");

            if (connectionCount <= 0)
            {
                _logger.LogError("connectionCount is less than 1");
                throw new Exception("connectionCount cannot be less than 1");
            }
            _connectionCount = connectionCount;

            using (_con = new Connection())
            {
                _mainBackgroundTask = Task.Factory.StartNew(process); // Start the processing threads.
            }
        }

        #region Request Management

        public void sendRequest(Request request)
        {
            try
            {
                if (_queue == null || _queue.IsCompleted)
                {
                    _logger.LogDebug($"sendRequest(starting new queue)");
                    _queue = new BlockingCollection<Request>();
                }
                _queue.Add(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw ex;
            }
        }

        private void processQueue()
        {
            if (_queue == null) return;

            foreach (Request request in _queue.GetConsumingEnumerable())
            {
                try
                {
                    _logger.LogDebug("RequestsCommand={0}", request.Command);
                    lock (_lock)
                    {
                        _result += _con.runCommand(request.Command);
                        _logger.LogDebug("Partial result={0}", _result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }
        }

        private void process()
        {
            var actions = Enumerable.Repeat<Action>(processQueue, _connectionCount);
            Parallel.Invoke(actions.ToArray());
        }

        #endregion Request Management

        public int getSummary()
        {
            _queue.CompleteAdding();
            _mainBackgroundTask.Wait();

            _logger.LogInformation($"getSummary._result={_result}");
            return _result;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                builder.AddNLog("nlog.config");
            });
        }
    }
}
