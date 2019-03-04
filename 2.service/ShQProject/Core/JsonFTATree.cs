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
        [Required]
        public Guid ProjectId { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public List<JsonFTAProperties> FTAProperties = new List<JsonFTAProperties>();

        [Required]
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
    public class Mode
    {
        [Required]
        public string Size { get; set; }
        [Required]
        public string Shape { get; set; }
        [Required]
        public string Color { get; set; }
        [Required]
        public int X { get; set; }
        [Required]
        public int Y { get; set; }
        [Required]
        public int Index { get; set; }
        public string Note { get; set; }
    }

    public class JsonFTANode
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string ItemType { get; set; }
        public string Des { get; set; }

        [Required]
        public Mode Mode { get; set; }
    }


    public class JsonFTAEdge
    {
        [Required]
        public string Source { get; set; }
        [Required]
        public int SourceAnchor { get; set; }
        [Required]
        public string Target { get; set; }
        [Required]
        public int TargetAnchor { get; set; }
        [Required]
        public string Id { get; set; }
        [Required]
        public int Index { get; set; }
    }
}