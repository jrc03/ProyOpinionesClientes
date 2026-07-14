document.addEventListener('DOMContentLoaded', () => {
    // Inicializar iconos de Lucide
    if (window.lucide) {
        lucide.createIcons();
    }

    // Instancias de Gráficos (para poder destruirlos al recargar si fuera necesario)
    let chartTendencia = null;
    let chartSentimiento = null;
    let chartProductos = null;

    // Cargar datos del dashboard
    initDashboard();

    async function initDashboard() {
        try {
            const statusRes = await fetch('/api/dashboard/status');
            const statusData = await statusRes.json();
            
            const pill = document.getElementById('db-status-pill');
            const pillText = document.getElementById('db-status-text');
            const errorBanner = document.getElementById('error-banner');

            if (statusData.connected) {
                pill.className = 'status-indicator status-connected';
                pillText.textContent = 'Conectado a SQL Server';
                errorBanner.classList.add('hidden');
            } else {
                pill.className = 'status-indicator status-disconnected';
                pillText.textContent = 'Desconectado';
                errorBanner.classList.remove('hidden');
                if (statusData.error) {
                    document.getElementById('error-message').textContent = 
                        `No se pudo conectar a la base de datos (${statusData.error}). Se muestran datos en cero.`;
                }
            }

            // Llamar a cargar los endpoints
            await Promise.all([
                cargarKPIs(),
                cargarClasificacion(),
                cargarProductos(),
                cargarTendencia()
            ]);

        } catch (err) {
            console.error('Error al inicializar el dashboard:', err);
            mostrarErrorGenerico();
        }
    }

    async function cargarKPIs() {
        try {
            const res = await fetch('/api/dashboard/kpis');
            const data = await res.json();
            
            document.getElementById('kpi-total').textContent = formatNumber(data.total);
            document.getElementById('kpi-positivas').textContent = formatNumber(data.positivas);
            document.getElementById('kpi-negativas').textContent = formatNumber(data.negativas);
            document.getElementById('kpi-neutras').textContent = formatNumber(data.neutras);
            document.getElementById('kpi-satisfaccion').textContent = 
                data.satisfaccionGlobal !== null ? `${data.satisfaccionGlobal.toFixed(1)}%` : '0.0%';
        } catch (err) {
            console.error('Error al cargar KPIs:', err);
            setKPIsEmpty();
        }
    }

    async function cargarClasificacion() {
        try {
            const res = await fetch('/api/dashboard/clasificacion');
            const data = await res.json(); // [ { clasificacion: "Positiva", cantidad: X }, ... ]

            const labels = ['Positiva', 'Negativa', 'Neutra'];
            const valores = [0, 0, 0];

            data.forEach(item => {
                const index = labels.indexOf(item.clasificacion);
                if (index !== -1) {
                    valores[index] = item.cantidad;
                }
            });

            const ctx = document.getElementById('chart-sentimiento').getContext('2d');
            if (chartSentimiento) chartSentimiento.destroy();

            chartSentimiento = new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        data: valores,
                        backgroundColor: [
                            '#198754', // Verde (Positivo)
                            '#dc3545', // Rojo (Negativo)
                            '#6c757d'  // Gris (Neutro)
                        ],
                        borderWidth: 1,
                        borderColor: '#ffffff'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                boxWidth: 12,
                                padding: 15
                            }
                        }
                    }
                }
            });

        } catch (err) {
            console.error('Error al cargar clasificación:', err);
        }
    }

    async function cargarProductos() {
        try {
            const res = await fetch('/api/dashboard/productos');
            const data = await res.json(); // [ { idProducto, nombreProducto, totalOpiniones, porcentajeSatisfaccion }, ... ]

            const labels = data.map(item => item.nombreProducto || item.idProducto);
            const opiniones = data.map(item => item.totalOpiniones);
            const satisfaccion = data.map(item => item.porcentajeSatisfaccion);

            const ctx = document.getElementById('chart-productos').getContext('2d');
            if (chartProductos) chartProductos.destroy();

            chartProductos = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Total de Opiniones',
                            data: opiniones,
                            backgroundColor: '#0d6efd',
                            borderWidth: 0,
                            yAxisID: 'y'
                        },
                        {
                            label: '% Satisfacción',
                            data: satisfaccion,
                            type: 'line',
                            borderColor: '#6f42c1',
                            backgroundColor: 'transparent',
                            borderWidth: 2,
                            pointRadius: 3,
                            yAxisID: 'y1'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            type: 'linear',
                            display: true,
                            position: 'left',
                            title: {
                                display: true,
                                text: 'Opiniones'
                            },
                            grid: {
                                drawOnChartArea: true
                            }
                        },
                        y1: {
                            type: 'linear',
                            display: true,
                            position: 'right',
                            min: 0,
                            max: 100,
                            title: {
                                display: true,
                                text: 'Satisfacción %'
                            },
                            grid: {
                                drawOnChartArea: false // Evita solapamiento de líneas de grid
                            }
                        }
                    },
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });

        } catch (err) {
            console.error('Error al cargar productos:', err);
        }
    }

    async function cargarTendencia() {
        try {
            const res = await fetch('/api/dashboard/tendencia');
            const data = await res.json(); // [ { mes, totalOpiniones, porcentajeSatisfaccion }, ... ]

            // Ordenar por fecha del mes si no viene ordenado
            data.sort((a, b) => new Date(a.mes) - new Date(b.mes));

            const labels = data.map(item => formatMes(item.mes));
            const satisfaccion = data.map(item => item.porcentajeSatisfaccion);
            const opiniones = data.map(item => item.totalOpiniones);

            const ctx = document.getElementById('chart-tendencia').getContext('2d');
            if (chartTendencia) chartTendencia.destroy();

            chartTendencia = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Satisfacción Promedio (%)',
                            data: satisfaccion,
                            borderColor: '#6f42c1',
                            backgroundColor: 'transparent',
                            borderWidth: 2.5,
                            pointBackgroundColor: '#6f42c1',
                            pointRadius: 4,
                            tension: 0.1,
                            yAxisID: 'y'
                        },
                        {
                            label: 'Opiniones del Mes',
                            data: opiniones,
                            type: 'bar',
                            backgroundColor: '#e2e3e5',
                            borderWidth: 0,
                            yAxisID: 'y1'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            type: 'linear',
                            display: true,
                            position: 'left',
                            min: 0,
                            max: 100,
                            title: {
                                display: true,
                                text: 'Satisfacción %'
                            }
                        },
                        y1: {
                            type: 'linear',
                            display: true,
                            position: 'right',
                            title: {
                                display: true,
                                text: 'Volumen de Opiniones'
                            },
                            grid: {
                                drawOnChartArea: false
                            }
                        }
                    },
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });

        } catch (err) {
            console.error('Error al cargar tendencia:', err);
        }
    }

    function formatNumber(num) {
        if (num === null || num === undefined) return '0';
        return num.toLocaleString();
    }

    function formatMes(fechaStr) {
        if (!fechaStr) return '';
        const fecha = new Date(fechaStr);
        if (isNaN(fecha.getTime())) return fechaStr;
        // Retornar formato "Mes AAAA" simple
        const opciones = { month: 'short', year: 'numeric' };
        return fecha.toLocaleDateString('es-ES', opciones);
    }

    function setKPIsEmpty() {
        document.getElementById('kpi-total').textContent = '0';
        document.getElementById('kpi-positivas').textContent = '0';
        document.getElementById('kpi-negativas').textContent = '0';
        document.getElementById('kpi-neutras').textContent = '0';
        document.getElementById('kpi-satisfaccion').textContent = '0.0%';
    }

    function mostrarErrorGenerico() {
        const banner = document.getElementById('error-banner');
        banner.classList.remove('hidden');
        document.getElementById('error-message').textContent = 
            'Ocurrió un error inesperado al conectar con el servidor backend.';
        setKPIsEmpty();
    }
});
