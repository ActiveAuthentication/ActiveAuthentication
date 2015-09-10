using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ActiveAuthenticationDesktopClient
{
    public abstract class Verifier
    {
        public abstract void Verify(DataSet dsFeatures, DataSet dsFeaturesInliers, DataSet dsFeatureLabelGrouped, DataSet dsTemplate, DataSet dsVerifyResults);
    }
}
