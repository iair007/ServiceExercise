using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConnectionPool
{
    public class RequestSender
    {

        private IService _service;

        public RequestSender(IService controller)
        {
            _service = controller;
        }

        public void sendRequests()
        {
            for (int i = 0; i < 1_000; i++)
            {
                _service.sendRequest(new Request { Command = i });

                if (i % 10 == 0)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
