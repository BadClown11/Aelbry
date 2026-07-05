window.GanttView = (function () {
    let ganttInstance = null;

    function projectId() {
        return document.getElementById('filterProjectId').value;
    }

    function flatten(nodes, list) {
        nodes.forEach((n) => {
            list.push(n);
            flatten(n.children ?? [], list);
        });
        return list;
    }

    function formatDate(date) {
        return date.toISOString().substring(0, 10);
    }

    function resolveDates(activity) {
        if (activity.estimatedStartDate && activity.estimatedEndDate) {
            return {
                start: activity.estimatedStartDate.substring(0, 10),
                end: activity.estimatedEndDate.substring(0, 10),
            };
        }

        const durationDays = activity.estimatedHours > 0 ? Math.max(1, Math.round(activity.estimatedHours / 8)) : 1;
        const start = new Date();
        const end = new Date();
        end.setDate(end.getDate() + durationDays);

        return { start: formatDate(start), end: formatDate(end) };
    }

    async function loadAll() {
        const pid = projectId();
        if (!pid) return;

        const treeResult = await Aelbry.api.get(`/Activity/GetTreeByProject?projectId=${pid}`);
        console.log('[Gantt] GetTreeByProject ->', treeResult);
        const activities = treeResult.result === 'OK' ? flatten(treeResult.data, []) : [];

        if (activities.length === 0) {
            document.getElementById('ganttChart').innerHTML = '';
            return;
        }

        const dependencyEntries = await Promise.all(activities.map(async (a) => {
            const r = await Aelbry.api.get(`/Activity/GetDependencies?activityId=${a.activityId}`);
            return [a.activityId, r.result === 'OK' ? r.data.map((d) => `activity-${d.dependsOnActivityId}`) : []];
        }));
        const dependenciesByActivity = Object.fromEntries(dependencyEntries);

        const criticalPathResult = await Aelbry.api.get(`/Activity/GetCriticalPath?projectId=${pid}`);
        const criticalIds = criticalPathResult.result === 'OK'
            ? new Set(criticalPathResult.data.filter((c) => c.isCritical).map((c) => c.activityId))
            : new Set();

        const tasks = activities.map((a) => {
            const { start, end } = resolveDates(a);
            return {
                id: `activity-${a.activityId}`,
                name: `${a.code} - ${a.name}`,
                start,
                end,
                progress: a.progressPercentage ?? 0,
                dependencies: (dependenciesByActivity[a.activityId] ?? []).join(','),
                custom_class: criticalIds.has(a.activityId) ? 'gantt-critical' : '',
            };
        });

        document.getElementById('ganttChart').innerHTML = '';
        ganttInstance = new Gantt('#ganttChart', tasks, {
            view_mode: document.getElementById('viewMode').value,
            on_date_change: async (task, start, end) => {
                const activityId = parseInt(task.id.replace('activity-', ''), 10);
                const result = await Aelbry.api.post(`/Activity/UpdateDates?activityId=${activityId}&estimatedStartDate=${formatDate(start)}&estimatedEndDate=${formatDate(end)}`);
                if (result.result !== 'OK') alert(result.result);
            },
        });
    }

    function changeViewMode() {
        if (ganttInstance) {
            ganttInstance.change_view_mode(document.getElementById('viewMode').value);
        }
    }

    Aelbry.projectContext.autoLoad(loadAll);

    return { loadAll, changeViewMode };
})();
