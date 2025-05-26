using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Celesta.Serialization;
using SonicAudioLib.CriMw.Serialization;

namespace Celesta.BuilderNodes
{
    public class AisacNode : BuilderBaseNode
    {
        public float Progress;

        [Category("General"), DisplayName("Aisac Name")]
        [Description("Representative name of this aisac.")]
        public string AisacName { get; set; }

        [Category("General")]
        [Description("Type of this aisac. Values are unknown.")]
        public byte Type { get; set; }

        [Category("Graph")]
        public List<BuilderAisacGraphNode> Graphs { get; set; } = new List<BuilderAisacGraphNode>();

        [Category("Graph"), DisplayName("Random Range")]
        public byte RandomRange { get; set; }

        public AisacNode(SerializationAisacTable aisacTable)
        {
            Path = aisacTable.PathName;
            AisacName = aisacTable.Name;
            Type = aisacTable.Type;
            RandomRange = aisacTable.RandomRange;

            // Deserialize the graphs
            List<SerializationAisacGraphTable> graphTables = CriTableSerializer.Deserialize<SerializationAisacGraphTable>(aisacTable.Graph);
            foreach (SerializationAisacGraphTable graphTable in graphTables)
            {
                BuilderAisacGraphNode graphNode = new BuilderAisacGraphNode();
                graphNode.Path = $"Graph{Graphs.Count}";
                graphNode.Type = graphTable.Type;
                graphNode.MaximumX = graphTable.InMax;
                graphNode.MinimumX = graphTable.InMin;
                graphNode.MaximumY = graphTable.OutMax;
                graphNode.MinimumY = graphTable.OutMin;

                // Deserialize the points
                List<SerializationAisacPointTable> pointTables = CriTableSerializer.Deserialize<SerializationAisacPointTable>(graphTable.Points);
                foreach (SerializationAisacPointTable pointTable in pointTables)
                {
                    BuilderAisacPointNode pointNode = new BuilderAisacPointNode();
                    pointNode.Path = $"Point{graphNode.Points.Count}";
                    pointNode.X = pointTable.In;
                    pointNode.Y = pointTable.Out;
                    graphNode.Points.Add(pointNode);
                }

                Graphs.Add(graphNode);
            }

        }
    }
}
