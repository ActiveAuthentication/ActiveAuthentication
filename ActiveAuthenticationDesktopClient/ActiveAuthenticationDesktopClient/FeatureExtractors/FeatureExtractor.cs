using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ActiveAuthenticationDesktopClient.FeatureExtractors
{
    abstract class FeatureExtractor
    {
        protected FeatureExtractorArguments arguments;
        public abstract DataTable extract(DataTable KeyboardEvents);

        public FeatureExtractor(FeatureExtractorArguments fea)
        {
            arguments = fea;
        }
    }
}
