using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class ParametrosGrillaDto<T>
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<FilterParam> Filters { get; set; }
        public string FilterText { get; set; }
        public string Filter { get; set; }
        public object[] FilterParameters { get; set; }
        public string ExtraData { get; set; }

        public T Dato { get; set; }
        public string Sort { get; set; }
        public string Group { get; set; }
        public string Aggregate { get; set; }

        //public List<SortParam> Sort { get; set; }

        //Para búsqueda bandejas
        public int? DocumentoIngreso { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? UnidadTecnica { get; set; }
        public int? Estado { get; set; }

    }

    public class FilterParam
    {
        public string Field { get; set; }
        public string Member { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }
        public string IgnoreCase { get; set; }
    }

    public class SortParam
    {
        public string Field { get; set; }
        public string Dir { get; set; }
    }

    public class GroupResult
    {
        public object Key { get; set; }
        public string Member { get; set; }
        public IEnumerable Items { get; set; }
        public bool HasSubgroups { get; set; }
        public int SubgroupCount { get; set; }
        public object[] Subgroups { get; set; }
        public object Aggregates { get; set; }
        public object AggregateFunctionsProjection { get; set; }
        //public Aggregates Aggregates { get; set; }
        //public bool HasSubgroups { get; set; }
        //public string Member { get; set; }
        public int ItemCount { get; set; }
        //public string Key { get; set; }
        //public List<Object> Items { get; set; }
    }

    //public class AggregateFunctionsProjection
    //{
    //    public int Count_AnnoMes { get; set; }
    //    public int Count_RemitenteGenero { get; set; }
    //}

    //public class Aggregates
    //{
    //    public Aggregate AnnoMes { get; set; }
    //    public Aggregate RemitenteGenero { get; set; }
    //}

    //public class Aggregate
    //{
    //    public int Count { get; set; }
    //}

    //public class AggregateResults
    //{
    //    public int Value { get; set; }
    //    public string Member { get; set; }
    //    public int FormattedValue { get; set; }
    //    public int ItemCount { get; set; }
    //    public string Caption { get; set; }
    //    public string FunctionName { get; set; }
    //    public string Count { get; set; }
    //}

}
