using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Helpers.Consumer
{
    /// <summary>
    /// Centralized geometry calculations for patrol route distance estimation
    /// </summary>
    public static class GeometryHelper
    {
        /// <summary>
        /// Parse area_shape JSON and extract center coordinates (x, y) in meters
        /// </summary>
        /// <returns>Center coordinates or null if parsing fails</returns>
        public static (double x, double y)? ExtractCenterPoint(string? areaShapeJson)
        {
            if (string.IsNullOrWhiteSpace(areaShapeJson))
                return null;

            try
            {
                var doc = JsonDocument.Parse(areaShapeJson);
                var elements = doc.RootElement.EnumerateArray().ToList();

                // Try to find center point first
                foreach (var elem in elements)
                {
                    // Check for "type": "center" or "center": true
                    if (elem.TryGetProperty("type", out var type) && type.GetString() == "center")
                    {
                        if (elem.TryGetProperty("x", out var x) && elem.TryGetProperty("y", out var y))
                        {
                            return (x.GetDouble(), y.GetDouble());
                        }
                    }
                    if (elem.TryGetProperty("center", out var isCenter) && isCenter.GetBoolean())
                    {
                        if (elem.TryGetProperty("x", out var x) && elem.TryGetProperty("y", out var y))
                        {
                            return (x.GetDouble(), y.GetDouble());
                        }
                    }
                }

                // Fallback: Calculate geometric centroid from all corner points
                double sumX = 0, sumY = 0;
                int count = 0;

                foreach (var elem in elements)
                {
                    if (elem.TryGetProperty("x", out var x) && elem.TryGetProperty("y", out var y))
                    {
                        sumX += x.GetDouble();
                        sumY += y.GetDouble();
                        count++;
                    }
                }

                return count > 0 ? (sumX / count, sumY / count) : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Calculate Euclidean distance between two points
        /// </summary>
        public static double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        /// <summary>
        /// Calculate cumulative distances for ordered route areas
        /// </summary>
        /// <param name="orderedAreas">List of (AreaId, OrderIndex, AreaShape) sorted by OrderIndex</param>
        /// <returns>Dictionary mapping OrderIndex to cumulative distance</returns>
        public static Dictionary<int, float> CalculateRouteDistances(
            List<(Guid AreaId, int OrderIndex, string? AreaShape)> orderedAreas)
        {
            var result = new Dictionary<int, float>();
            if (!orderedAreas.Any())
                return result;

            double cumulativeDistance = 0;
            (double x, double y)? prevCenter = null;

            foreach (var (areaId, orderIndex, areaShape) in orderedAreas)
            {
                var currentCenter = ExtractCenterPoint(areaShape);

                if (prevCenter.HasValue && currentCenter.HasValue)
                {
                    // Calculate distance from previous area
                    var segmentDistance = CalculateDistance(
                        prevCenter.Value.x, prevCenter.Value.y,
                        currentCenter.Value.x, currentCenter.Value.y
                    );
                    cumulativeDistance += segmentDistance;
                }

                result[orderIndex] = (float)cumulativeDistance;
                prevCenter = currentCenter;
            }

            return result;
        }
    }
}
