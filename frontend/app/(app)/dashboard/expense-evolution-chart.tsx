"use client";

import { Box, Typography, useTheme } from "@mui/material";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { useChartPalette } from "../../components/chartPalette";

export type EvolutionItem = { year: number; month: number; expense: number };

const MONTH_LABELS = ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];

const tooltipStyle = { borderRadius: 8, fontSize: 13 };

export default function ExpenseEvolutionChart({ evolution, currency }: { evolution: EvolutionItem[]; currency: string }) {
  const palette = useChartPalette();
  const { palette: mui } = useTheme();
  const hasData = evolution.some((m) => m.expense > 0);
  const fmt = (v: number) => `${v.toLocaleString("es-AR", { maximumFractionDigits: 2 })} ${currency}`;
  const chartData = evolution.map((m) => ({ label: MONTH_LABELS[m.month - 1], Gastos: m.expense }));

  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Evolución de gastos (últimos 6 meses)</Typography>
      {!hasData ? (
        <Typography color="text.secondary">Sin movimientos en el período.</Typography>
      ) : (
        <ResponsiveContainer width="100%" height={260}>
          <BarChart data={chartData} margin={{ top: 8, right: 8 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={mui.divider} vertical={false} />
            <XAxis dataKey="label" tick={{ fontSize: 12, fill: mui.text.secondary }} />
            <YAxis tick={{ fontSize: 12, fill: mui.text.secondary }} width={70} />
            <Tooltip
              formatter={(v) => [fmt(Number(v)), "Gastos"]}
              contentStyle={tooltipStyle}
              cursor={{ fill: mui.action.hover, opacity: 0.4 }}
            />
            <Bar dataKey="Gastos" fill={palette.expense} radius={[6, 6, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      )}
    </Box>
  );
}
