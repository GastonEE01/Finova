"use client";

import { Box, Typography, useTheme } from "@mui/material";
import { Bar, BarChart, CartesianGrid, Legend, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";
import { useChartPalette } from "../../components/chartPalette";

const MONTH_LABELS = ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];

type MonthItem = { year: number; month: number; income: number; expense: number };
type Response = { currency: string; months: MonthItem[] };

const tooltipStyle = { borderRadius: 8, fontSize: 13 };

export default function IncomeVsExpensesChart({ currency, year, month }: { currency: string; year: number; month: number }) {
  const router = useRouter();
  const palette = useChartPalette();
  const { palette: mui } = useTheme();
  const [data, setData] = useState<Response | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    let cancelled = false;
    Promise.resolve().then(() => {
      if (cancelled) return;
      setData(null);
      setError("");
    });
    apiFetch(`/api/dashboard/income-vs-expenses?currency=${encodeURIComponent(currency)}&year=${year}&month=${month}`)
      .then(async (res) => {
        if (cancelled) return;
        if (res.status === 401) { router.push("/login"); return; }
        if (res.ok) setData(await res.json());
        else setError("No se pudo cargar el gráfico.");
      })
      .catch(() => { if (!cancelled) setError("No se pudo cargar el gráfico."); });
    return () => { cancelled = true; };
  }, [currency, year, month, router]);

  const hasData = data && data.months.some((m) => m.income > 0 || m.expense > 0);
  const fmt = (v: number) => `${v.toLocaleString("es-AR", { maximumFractionDigits: 2 })} ${currency}`;
  const chartData = data
    ? data.months.map((m) => ({
        label: MONTH_LABELS[m.month - 1],
        Ingresos: m.income,
        Gastos: m.expense,
      }))
    : [];

  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Ingresos vs. gastos (últimos 6 meses)</Typography>
      {error && <Typography color="error">{error}</Typography>}
      {!error && !data && <Typography color="text.secondary">Cargando gráfico...</Typography>}
      {data && !hasData && (
        <Typography color="text.secondary">Sin movimientos en el período.</Typography>
      )}
      {data && hasData && (
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={chartData} margin={{ top: 8, right: 8 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={mui.divider} vertical={false} />
            <XAxis dataKey="label" tick={{ fontSize: 12, fill: mui.text.secondary }} />
            <YAxis tick={{ fontSize: 12, fill: mui.text.secondary }} width={70} />
            <Tooltip
              formatter={(v, name) => [fmt(Number(v)), name]}
              contentStyle={tooltipStyle}
              cursor={{ fill: mui.action.hover, opacity: 0.4 }}
            />
            <Legend wrapperStyle={{ fontSize: 12 }} />
            <Bar dataKey="Ingresos" fill={palette.income} radius={[6, 6, 0, 0]} />
            <Bar dataKey="Gastos" fill={palette.expense} radius={[6, 6, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      )}
    </Box>
  );
}
