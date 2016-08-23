namespace NetduinoSerialSDK
{
    internal class LimitQueue : System.Collections.Queue
    {
        private int _limit;
        public LimitQueue(int limit = 3) : base()
        {
            _limit = limit;
        }

        public override void Enqueue(object obj)
        {
            if (this.Count == _limit) this.Dequeue();
            base.Enqueue(obj);
        }
    }
}
