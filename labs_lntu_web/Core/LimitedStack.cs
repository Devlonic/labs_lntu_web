using System.Collections;

namespace labs_lntu_web.Core {
    public class LimitedStack<T> : IEnumerable<T?> {
        private readonly T?[] arr = new T[6];
        private int size = 0;
        public void Push(T item) {
            for ( int i = arr.Length - 1; i > 0; i-- ) {
                arr[i] = arr[i - 1];
            }
            arr[0] = item;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.arr.GetEnumerator();
        }
        public IEnumerator<T?> GetEnumerator() {
            return ( (IEnumerable<T?>)this.arr ).GetEnumerator();
        }
    }
}
