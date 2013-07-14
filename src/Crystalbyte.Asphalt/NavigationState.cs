using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Asphalt {
    public static class NavigationState {

        private static readonly Stack<object> ArgumentStack = new Stack<object>();

        public static void Push<T>(T argument) {
            ArgumentStack.Push(argument);
        }

        public static object Pop() {
            return ArgumentStack.Pop();
        }

        public static object Peek() {
            return ArgumentStack.Peek();
        }

        public static bool IsEmpty {
            get { return ArgumentStack.Count == 0; }
        }
    }
}
