using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ratzap
{
#if true
    public class AYPart : PartModule
    {
        private void Start()
        {
            Log.Info("AYPart.Start");
        }
    }

    public class AYCrewPart : PartModule, IResourceConsumer
    {
        private void Start()
        {
            Log.Info("AYCrewPart.Start");
        }
        public List<PartResourceDefinition> GetConsumedResources()
        {
            return null;
        }
    }
#endif
}
