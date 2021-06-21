using ServiceExercise;
using System;

namespace ConnectionPool {
    public class Program {

        private const int CONNETION_COUNT = 4;

        static void Main(string[] args) {

            IService service = new Service(CONNETION_COUNT);
            DateTime timer = DateTime.Now;

            RequestSender requestSender = new RequestSender(service);

            requestSender.sendRequests();

            DateTime gettingSummaryTimer = DateTime.Now;
            int result = service.getSummary();
            TimeSpan getSummaryTimeSpan = DateTime.Now - gettingSummaryTimer;

            TimeSpan totalTimeSpan = DateTime.Now - timer;

            Console.WriteLine($"the result is {result}");
            Console.WriteLine($"Total time wating for summary: {getSummaryTimeSpan.TotalSeconds} seconds");
            Console.WriteLine($"Total Running time: {totalTimeSpan.TotalSeconds} seconds");

        }
    }
}
