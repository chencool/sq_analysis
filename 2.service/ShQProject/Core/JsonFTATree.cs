using Dxc.Shq.WebApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Dxc.Shq.WebApi.Core
{
    public class JsonFTATree
    {
        [JsonProperty(PropertyName = "attributes")]
        public List<JsonFTAProperties> FTAProperties = new List<JsonFTAProperties>();

        [JsonProperty(PropertyName = "nodes")]
        public List<JsonFTANode> FTANodes = new List<JsonFTANode>();

        [JsonProperty(PropertyName = "edges")]
        public List<JsonFTAEdge> FTAEdges = new List<JsonFTAEdge>();

        public static JsonFTATree ToFTATree(string content)
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            return (JsonFTATree)json_serializer.DeserializeObject(content);
        }
    }

    public class JsonFTAProperties
    {
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 失效概率Q
        /// </summary>
        public double FailureRateQ { get; set; }

        /// <summary>
        /// 失效率 lambda
        /// </summary>
        public double InvalidRate { get; set; }

        /// <summary>
        /// 故障时间
        /// </summary>
        public double FailureTime { get; set; }

        /// <summary>
        /// 单点故障诊断覆盖率
        /// </summary>
        public double DCrf { get; set; }

        /// <summary>
        /// 潜伏故障诊断覆盖率
        /// </summary>
        public double DClf { get; set; }

        /// <summary>
        /// 参考失效率 q
        /// </summary>
        public double ReferenceFailureRateq { get; set; }
    }
    
        

    public class JsonFTANode
    {
        [Required]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string ItemType { get; set; }
        public string Des { get; set; }
        public string Size { get; set; }
        public string Shape { get; set; }
        public string Color { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Index { get; set; }
        public string Node { get; set; }
        public double SmallFailureRateQ { get; set; }
    }


    public class JsonFTAEdge
    {
        [Required]
        public string Source { get; set; }
        public int SourceAnchor { get; set; }
        [Required]
        public string Target { get; set; }
        public int TargetAnchor { get; set; }
        public string Id { get; set; }
        public int Index { get; set; }
    }

    public class JsonNodeEvent
    {
        public string NodeId { get; set; }

        public int EventId { get; set; }
        public int FalureId { get; set; }

        public double EventValue { get; set; }

        public int EventValueType { get; set; }
    }

    public class JsonTreeEvent
    {
        public int EventId { get; set; }
        public int FalureId { get; set; }
        public double FailureRateQ { get; set; }
        public double InvalidRate { get; set; }
    }
}