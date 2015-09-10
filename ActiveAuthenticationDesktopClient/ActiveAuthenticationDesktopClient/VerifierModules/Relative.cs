using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActiveAuthenticationDesktopClient
{
    public class Relative : Verifier
    {

        int minMatchingPairs = 10;

        #region IVerifierModule Members
        public string VerifierType
        {
            get { return "Relative"; }
        }

        public override void Verify(DataSet dsFeatures, DataSet dsFeaturesInliers, DataSet dsFeatureLabelGrouped, DataSet dsTemplate, DataSet dsVerifyResults)
        {
            DataTable dtFeaturesInliers = dsFeatures.Tables[0];
            DataTable dtTemplate = dsTemplate.Tables[0];

            try { 
                //the template grouped by FeatureType
                var templateGrouped = from featureValue in dtTemplate.AsEnumerable()
                                      group new
                                      {
                                          FeatureType = featureValue.Field<string>("FeatureType"),
                                          FeatureLabel = featureValue.Field<string>("FeatureLabel"),
                                          FeatureMean = featureValue.Field<double>("Mean")
                                      } by featureValue["FeatureType"] into resultGroup
                                      select new
                                      {
                                          resultGroup,
                                          FeatureType = resultGroup.FirstOrDefault().FeatureType,
                                      };

                //the sample grouped by FeatureType
                var sampleGrouped = from featureValue in dtFeaturesInliers.AsEnumerable()
                                    group new
                                    {
                                        SecurityId = featureValue.Field<string>("SecurityId"),
                                        SampleId = featureValue.Field<string>("SampleId"),
                                        FeatureType = featureValue.Field<string>("FeatureType"),
                                        FeatureLabel = featureValue.Field<string>("FeatureLabel"),
                                        FeatureValue = featureValue.Field<double>("FeatureValue")
                                    } by featureValue["FeatureType"] into resultGroup
                                    select new
                                    {
                                        resultGroup,
                                        SecurityId = resultGroup.FirstOrDefault().SecurityId,
                                        SampleId = resultGroup.FirstOrDefault().SampleId,
                                        FeatureType = resultGroup.FirstOrDefault().FeatureType,
                                    };

                //join the tamplate and sample on feature type, the sub groups SampleGroup and TemplateGroup contain the actual FeatureValues
                var joined = from sample in sampleGrouped
                             join template in templateGrouped
                             on sample.FeatureType equals template.FeatureType
                             select new
                             {
                                 FeatureType = sample.FeatureType,
                                 SecurityId = sample.SecurityId,
                                 SampleId = sample.SampleId,
                                 SampleGroup = sample.resultGroup,
                                 TemplateGroup = template.resultGroup
                             };

                foreach (var row in joined)
                {
                    //the sample grouped by FeatureLabel to produce means for each FeatureLabel
                    var sampleMeaned = from sampleRows in row.SampleGroup
                                       group sampleRows by sampleRows.FeatureLabel into featureLabels
                                       select new
                                       {
                                           FeatureMean = featureLabels.Average(featureValue => featureValue.FeatureValue),
                                           FeatureLabel = featureLabels.Key
                                       };

                    //the sample ordered by mean and given row numbers, has to be like this to use the Select() that provides an index,
                    // order by FeatureLabel, then by FeatureMean so that if multiple features have the same mean, they will be ordered alphabetically
                    // join with template to only get features shared by the template and sample, commented out for verification
                    var sampleOrdered = sampleMeaned
                                        .OrderBy(t => t.FeatureLabel)
                                        .OrderByDescending(t => t.FeatureMean)
                                        .Join(row.TemplateGroup,
                                            s => s.FeatureLabel,
                                            t => t.FeatureLabel,
                                            (s, t) =>
                                                new { FeatureLabel = s.FeatureLabel, FeatureMean = s.FeatureMean})
                                        .Select((sampleRows, RowNumber) => new { sampleRows, RowNumber });


                    //the template ordered by mean and given row numbers, has to be like this to use the Select() that provides an index,
                    // order by FeatureLabel, then by FeatureMean so that if multiple features have the same mean, they will be ordered alphabetically
                    // join with sample to only get features shared by the template and sample, commented out for verification
                    var templateOrdered = row.TemplateGroup
                                          .OrderBy(t => t.FeatureLabel)
                                          .OrderByDescending(t => t.FeatureMean)
                                          .Join(sampleMeaned,
                                              t => t.FeatureLabel,
                                              s => s.FeatureLabel,
                                              (t, s) =>
                                                  new { FeatureLabel = t.FeatureLabel, FeatureMean = t.FeatureMean })
                                          .Select((templateRows, RowNumber) => new { templateRows, RowNumber });


                    int templatecount = 0;
                    foreach (var r in templateOrdered)
                    {
                        templatecount++;
                    }

                    //join the sample and template data on FeatureLabel
                    var sampleTemplateJoined = from sampleRows in sampleOrdered
                                               join templateRows in templateOrdered
                                               on sampleRows.sampleRows.FeatureLabel equals templateRows.templateRows.FeatureLabel
                                               select new
                                               {
                                                   FeatureLabel = sampleRows.sampleRows.FeatureLabel,
                                                   SampleMean = sampleRows.sampleRows.FeatureMean,
                                                   SampleRowNumber = sampleRows.RowNumber,
                                                   TemplateMean = templateRows.templateRows.FeatureMean,
                                                   TemplateRowNumber = templateRows.RowNumber
                                               };


                    //get the Relative Score
                    int sum = 0;
                    int count = 0;

                    foreach(var r in sampleTemplateJoined){
                        sum += Math.Abs((r.SampleRowNumber + 1) - (r.TemplateRowNumber + 1));
                        count++;
                    }

                    double scale = 0;

                    if (count % 2 == 0) //count of matching pairs is even
                    {
                        scale = Math.Pow(count, 2) / 2;
                    }
                    else //count of mathcing pairs is odd
                    {
                        scale = (Math.Pow(count, 2) - 1) / 2;
                    }

                    double score = sum / scale;

                    minMatchingPairs = Configuration.relativeMinMatchingPairs;

                    if (!Double.IsNaN(score) && !Double.IsInfinity(score) && count >= minMatchingPairs) //RelativeMinMatchingPairs "CONFIGURATION WIREUP" (default 10)
                    dsVerifyResults.Tables[0].Rows.Add(row.SecurityId, row.SampleId, VerifierType, row.FeatureType, score);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Thrown in Verifier: {0}: {1}", VerifierType.ToString(), e.Message);
            }
        }
        #endregion
    }
}
