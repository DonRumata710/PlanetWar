using Microsoft.AspNetCore.Http;
using System;
using System.Threading;

namespace LaunchServer.Controllers
{
    public class CometAsyncState : IAsyncResult
    {
        // Чтобы было где хранить все это и был создан  
        // класс-наследник от IAsyncResult  
        public HttpContext CurrentContext;
        public AsyncCallback AsyncCallback;
        public object ExtraData;
        public string ClientGuid;
        private Boolean _isCompleted;

        // Конструктор  
        public CometAsyncState(HttpContext context, AsyncCallback callback, object data)
        {
            CurrentContext = context;
            AsyncCallback = callback;
            ExtraData = data;
            _isCompleted = false;
        }

        // Завершим запрос  
        public void CompleteRequest()
        {
            // При завершении запроса просто выставим флаг  
            // что он завершен  
            // и вызовем callback  
            _isCompleted = true;
            if (AsyncCallback != null)
            {
                AsyncCallback(this);
            }
        }

        #region IAsyncResult Members  
        // И снова видим набор заглушек для интерфейса IAsyncResult  

        public Boolean CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
        }

        public object AsyncState
        {
            get
            {
                return ExtraData;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return new ManualResetEvent(false);
            }
        }
        #endregion
    }
}