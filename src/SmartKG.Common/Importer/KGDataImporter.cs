// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Newtonsoft.Json;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartKG.Common.Importer
{
    public class KGDataImporter
    {
        private string rootPath;
        private List<string> vertexFileNames = new List<string>();
        private List<string> edgeFileNames = new List<string>();

        private HashSet<string> vertexIds = new HashSet<string>();
        private HashSet<string> edgeIds = new HashSet<string>();

        public KGDataImporter(string rootPath)
        {
           
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                throw new Exception("Rootpath of KG files are invalid.");
            }

            this.rootPath = PathUtility.CompletePath(rootPath);

            string[] files = Directory.GetFiles(rootPath, "*.json").Select(Path.GetFileName).ToArray();

            foreach (string file in files)
            {
                if (file.Contains("Edge"))
                {
                    this.edgeFileNames.Add(file);
                }
                else if (file.Contains("Vertex"))
                {
                    this.vertexFileNames.Add(file);
                }
            }
        }

        
        public List<Edge> ParseKGEdges()
        {
            List<Edge> edges = new List<Edge>();

            foreach(string file in this.edgeFileNames)
            {
                edges.AddRange(ParseKGEdges(file));
            }

            return edges;
        }

        private List<Edge> ParseKGEdges(string filename)
        {
            List<Edge> output = new List<Edge>();

            string fileName = rootPath + filename;
            string content = File.ReadAllText(fileName);

            List<Edge> edges = JsonConvert.DeserializeObject<List<Edge>>(content);

            foreach (Edge edge in edges)
            {                
                output.Add(edge);
            }

            return output;
        }

        public List<Vertex> ParseKGVertexes()
        {
            List<Vertex> vertexes = new List<Vertex>();
            foreach(string file in this.vertexFileNames)
            {
                vertexes.AddRange(this.ParseKGVertexes(file));
            }
            return vertexes;
        }

        private List<Vertex> ParseKGVertexes(string filename)
        {
            List<Vertex> output = new List<Vertex>();

            string fileName = rootPath + filename;
            string content = File.ReadAllText(fileName);

            List<Vertex> vertexes = JsonConvert.DeserializeObject<List<Vertex>>(content);

            foreach (Vertex vertex in vertexes)
            {
                if (vertexIds.Contains(vertex.id))
                {
                    Console.WriteLine(vertex.id);
                }
                else
                { 
                    vertexIds.Add(vertex.id);
                }

                output.Add(vertex);
            }

            return output;

        }        
    }
}
