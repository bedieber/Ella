using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ella;
using Ella.Attributes;
using Mono.Reflection;

namespace ReferenceReflector
{
    internal class Reflector
    {
        public void ReflectAssembly(Assembly a)
        {
            var types = a.GetTypes();
            var subscribersCode = ReflectSubscribers(types);
            var publishersCode = ReflectPublishers(types);
            Console.WriteLine("{0}\n{1}\n", subscribersCode, publishersCode);
        }

        private string ReflectPublishers(Type[] types)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var type in types)
            {
                var publishes = type.GetCustomAttributes(typeof(PublishesAttribute), false);
                if (!publishes.Any())
                    continue;
                var publishedTypes = from p in publishes select (p as PublishesAttribute).DataType;
                publishedTypes.ToList().ForEach(p =>
                {
                    sb.AppendFormat("{0} [shape=box color=green]\n", p.Name);
                    sb.AppendFormat("{0} -> {1}\n", type.Name, p.Name);
                });
            }
            return sb.ToString();
        }

        private string ReflectSubscribers(Type[] types)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var type in types.Where(t => t.GetCustomAttributes(typeof(SubscriberAttribute), false).Any()))
            {
                IEnumerable<MethodBase> methodInfos = type.GetMethods().Concat(type.GetMethods(BindingFlags.NonPublic));
                var privateStaticConstructors = type.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic);
                var privateInstanceConstructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                
                var constructors = type.GetConstructors().Concat(privateStaticConstructors).Concat(privateInstanceConstructors);
                methodInfos = methodInfos.Concat(constructors);

                foreach (var info in methodInfos)
                {
                    if (info.GetMethodBody() == null)
                        continue;
                    var instructions = info.GetInstructions();
                    var subscribecalls = from i in instructions
                                         let mi = (i.Operand as MethodInfo)
                                         where mi != null
                                         && mi.DeclaringType == typeof(Subscribe)
                                         && mi.Name == "To"
                                         select mi;
                    foreach (var subscribecall in subscribecalls)
                    {
                        sb.AppendLine(string.Format("{0} [shape=box, color=green]", subscribecall.GetGenericArguments()[0].Name));
                        sb.AppendLine(string.Format("{1} -> {0}", type.Name, subscribecall.GetGenericArguments()[0].Name));
                    }

                }
            }
            return sb.ToString();
        }
    }
}
