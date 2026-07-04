namespace Aelbry.BL
{
    /// <summary>
    /// Logica pura (sin acceso a datos) del metodo de camino critico (CPM) para el Gantt del
    /// Modulo 5. Asume dependencias Fin-a-Inicio; el BL es responsable de cargar las
    /// actividades/dependencias y de interpretar el resultado.
    /// </summary>
    public static class CriticalPathCalculator
    {
        public class ActivityNode
        {
            public int ActivityId { get; set; }

            public decimal Duration { get; set; }

            public List<int> PredecessorIds { get; set; } = new List<int>();
        }

        public class ActivityScheduleResult
        {
            public int ActivityId { get; set; }

            public decimal EarlyStart { get; set; }

            public decimal EarlyFinish { get; set; }

            public decimal LateStart { get; set; }

            public decimal LateFinish { get; set; }

            public decimal Slack { get; set; }

            public bool IsCritical { get; set; }
        }

        public static List<ActivityScheduleResult> Calculate(List<ActivityNode> nodes)
        {
            if (nodes.Count == 0)
            {
                return new List<ActivityScheduleResult>();
            }

            var byId = nodes.ToDictionary(n => n.ActivityId);
            var successors = nodes.ToDictionary(n => n.ActivityId, _ => new List<int>());

            foreach (var node in nodes)
            {
                foreach (var predecessorId in node.PredecessorIds)
                {
                    if (successors.TryGetValue(predecessorId, out var list))
                    {
                        list.Add(node.ActivityId);
                    }
                }
            }

            var order = TopologicalSort(nodes, byId, successors);

            var earlyStart = new Dictionary<int, decimal>();
            var earlyFinish = new Dictionary<int, decimal>();

            foreach (var id in order)
            {
                var node = byId[id];
                var validPredecessors = node.PredecessorIds.Where(byId.ContainsKey).ToList();

                decimal es = validPredecessors.Count == 0 ? 0m : validPredecessors.Max(p => earlyFinish[p]);

                earlyStart[id] = es;
                earlyFinish[id] = es + node.Duration;
            }

            decimal projectDuration = earlyFinish.Values.Max();

            var lateFinish = new Dictionary<int, decimal>();
            var lateStart = new Dictionary<int, decimal>();

            for (int i = order.Count - 1; i >= 0; i--)
            {
                int id = order[i];
                var node = byId[id];
                var succ = successors[id];

                decimal lf = succ.Count == 0 ? projectDuration : succ.Min(s => lateStart[s]);

                lateFinish[id] = lf;
                lateStart[id] = lf - node.Duration;
            }

            return order.Select(id =>
            {
                decimal slack = lateStart[id] - earlyStart[id];

                return new ActivityScheduleResult
                {
                    ActivityId = id,
                    EarlyStart = earlyStart[id],
                    EarlyFinish = earlyFinish[id],
                    LateStart = lateStart[id],
                    LateFinish = lateFinish[id],
                    Slack = slack,
                    IsCritical = slack == 0m,
                };
            }).ToList();
        }

        private static List<int> TopologicalSort(List<ActivityNode> nodes, Dictionary<int, ActivityNode> byId, Dictionary<int, List<int>> successors)
        {
            var inDegree = nodes.ToDictionary(n => n.ActivityId, n => n.PredecessorIds.Count(byId.ContainsKey));
            var queue = new Queue<int>(nodes.Where(n => inDegree[n.ActivityId] == 0).Select(n => n.ActivityId));
            var order = new List<int>();

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                order.Add(current);

                foreach (int successor in successors[current])
                {
                    inDegree[successor]--;
                    if (inDegree[successor] == 0)
                    {
                        queue.Enqueue(successor);
                    }
                }
            }

            if (order.Count != nodes.Count)
            {
                throw new InvalidOperationException("Se detecto un ciclo de dependencias; no se puede calcular el camino critico.");
            }

            return order;
        }
    }
}
