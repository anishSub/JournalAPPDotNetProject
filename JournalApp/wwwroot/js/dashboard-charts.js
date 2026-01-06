window.dashboardCharts = {
    charts: {},

    renderMoodPieChart: (elementId, data, isDark) => {
        const options = {
            series: data.map(d => d.value),
            chart: {
                type: 'donut',
                height: 250,
                background: 'transparent'
            },
            labels: data.map(d => d.name),
            colors: data.map(d => d.color),
            plotOptions: {
                pie: {
                    donut: {
                        size: '70%',
                        labels: {
                            show: true,
                            total: {
                                show: true,
                                label: 'Total',
                                formatter: function (w) {
                                    return "30d"
                                },
                                color: isDark ? '#fff' : '#374151'
                            }
                        }
                    }
                }
            },
            dataLabels: { enabled: false },
            legend: {
                position: 'right',
                offsetX: -20,
                labels: { colors: isDark ? '#fff' : '#374151' },
                formatter: function (seriesName, opts) {
                    try {
                        var series = opts.w.globals.series;
                        var total = series.reduce((a, b) => a + b, 0);
                        var value = series[opts.seriesIndex];
                        var percent = total > 0 ? (value / total) * 100 : 0;
                        return seriesName + " " + percent.toFixed(0) + "%";
                    } catch (e) { console.error(e); }
                    return seriesName;
                },
                itemMargin: {
                    vertical: 5
                }
            },
            stroke: { show: false },
            theme: { mode: isDark ? 'dark' : 'light' }
        };

        const currentEl = document.getElementById(elementId);
        if (window.dashboardCharts.charts[elementId]) {
            const existingChart = window.dashboardCharts.charts[elementId];
            if (existingChart.el !== currentEl) {
                existingChart.destroy();
            } else {
                existingChart.updateOptions(options); // Update full options or specific
                return;
            }
        }

        const chart = new ApexCharts(currentEl, options);
        chart.render();
        window.dashboardCharts.charts[elementId] = chart;
    },

    renderTagsBarChart: (elementId, data, isDark) => {
        const options = {
            series: [{
                name: 'Count',
                data: data.map(d => d.count)
            }],
            chart: {
                type: 'bar',
                height: 300,
                toolbar: { show: false },
                background: 'transparent'
            },
            colors: data.map(d => d.color),
            plotOptions: {
                bar: {
                    borderRadius: 4,
                    columnWidth: '50%',
                    distributed: true,
                }
            },
            dataLabels: { enabled: false },
            legend: { show: false },
            xaxis: {
                categories: data.map(d => d.name),
                labels: {
                    style: { colors: isDark ? '#9ca3af' : '#6b7280' }
                },
                axisBorder: { show: false },
                axisTicks: { show: false }
            },
            yaxis: {
                labels: {
                    style: { colors: isDark ? '#9ca3af' : '#6b7280' }
                }
            },
            grid: {
                borderColor: isDark ? '#374151' : '#e5e7eb',
                strokeDashArray: 4,
                yaxis: { lines: { show: true } }
            },
            theme: { mode: isDark ? 'dark' : 'light' },
            tooltip: {
                theme: isDark ? 'dark' : 'light',
                x: {
                    show: true,
                    formatter: function (val, opts) {
                        // Robust handling for distributed charts where val might be an object
                        if (typeof val === 'string') return val;
                        if (val && val.toString() !== '[object Object]') return val.toString();

                        // Fallback using data point index
                        if (opts && opts.w && opts.w.globals && opts.w.globals.labels && opts.dataPointIndex !== undefined) {
                            return opts.w.globals.labels[opts.dataPointIndex];
                        }

                        return "Tag";
                    }
                },
                y: {
                    title: {
                        formatter: function () {
                            return "Count:";
                        }
                    }
                }
            }
        };

        const currentEl = document.getElementById(elementId);
        if (window.dashboardCharts.charts[elementId]) {
            const existingChart = window.dashboardCharts.charts[elementId];
            if (existingChart.el !== currentEl) {
                existingChart.destroy();
            } else {
                existingChart.updateOptions(options);
                return;
            }
        }

        const chart = new ApexCharts(currentEl, options);
        chart.render();
        window.dashboardCharts.charts[elementId] = chart;
    },

    renderWordCountLineChart: (elementId, data, isDark) => {
        const options = {
            series: [{
                name: 'Words',
                data: data.map(d => d.words)
            }],
            chart: {
                type: 'line',
                height: 300,
                toolbar: { show: false },
                background: 'transparent'
            },
            colors: ['#3b82f6'],
            stroke: {
                curve: 'smooth',
                width: 3
            },
            markers: {
                size: 4,
                hover: { size: 6 }
            },
            xaxis: {
                categories: data.map(d => d.date),
                labels: {
                    style: { colors: isDark ? '#9ca3af' : '#6b7280' }
                },
                axisBorder: { show: false },
                axisTicks: { show: false }
            },
            yaxis: {
                labels: {
                    style: { colors: isDark ? '#9ca3af' : '#6b7280' }
                }
            },
            grid: {
                borderColor: isDark ? '#374151' : '#e5e7eb',
                strokeDashArray: 4,
                yaxis: { lines: { show: true } }
            },
            theme: { mode: isDark ? 'dark' : 'light' },
            tooltip: { theme: isDark ? 'dark' : 'light' }
        };

        const currentEl = document.getElementById(elementId);
        if (window.dashboardCharts.charts[elementId]) {
            const existingChart = window.dashboardCharts.charts[elementId];
            if (existingChart.el !== currentEl) {
                existingChart.destroy();
            } else {
                existingChart.updateOptions({
                    xaxis: {
                        categories: data.map(d => d.date)
                    },
                    series: [{
                        data: data.map(d => d.words)
                    }]
                });
                return;
            }
        }

        const chart = new ApexCharts(currentEl, options);
        chart.render();
        window.dashboardCharts.charts[elementId] = chart;
    }
};
