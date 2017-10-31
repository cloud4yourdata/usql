using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ADLAExt.Reducers
{
    public class SaidiRangeReducer : IReducer
    {
        const string BeginColName = "begin";
        const string EndColName = "end";
        const string SaidiColName = "saidi";
        private readonly int _maxDuration;
        public SaidiRangeReducer(int maxDuration)
        {
            _maxDuration = maxDuration;
        }
        public override IEnumerable<IRow> Reduce(IRowset input, IUpdatableRow output)
        {
            // Init aggregation values
            var firstRowProcessed = false;
            var begin = DateTime.MinValue;
            var end = DateTime.MinValue;
            var saidivalue = 0.0;
            // requires that the reducer is PRESORTED on begin and READONLY on the reduce key.
            foreach (var row in input.Rows)
            {
                if (!firstRowProcessed)
                {
                    firstRowProcessed = true;
                    begin = row.Get<DateTime>(BeginColName);
                    end = row.Get<DateTime>(EndColName);
                    saidivalue = row.Get<double>(SaidiColName);
                }
                else
                {
                    var b = row.Get<DateTime>("begin");
                    var e = row.Get<DateTime>("end");
                    var tmpsaidi = row.Get<double>("saidi");
                    if ((b - end).TotalSeconds <= _maxDuration)
                    {
                        saidivalue += tmpsaidi;
                    }
                    else
                    {
                        output.Set<double>("saidi", saidivalue);
                        output.Set<DateTime>("begin", begin);
                        output.Set<DateTime>("end", end);

                        yield return output.AsReadOnly();
                        saidivalue = tmpsaidi;
                        begin = b;
                    }
                    end = e;

                }
            }
            output.Set<DateTime>("begin", begin);
            output.Set<DateTime>("end", end);
            output.Set<double>("saidi", saidivalue);
            yield return output.AsReadOnly();
        }
    }
}