"use client";

import { Box, Typography } from "@mui/material";
import { BarChart } from "@mui/x-charts";

export type EvolutionItem = { year: number; month: number; expense: number };

const MONTH_LABELS = ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];

export default function ExpenseEvolutionChart({ evolution, currency }: { evolution: EvolutionItem[]; currency: string }) {
  const hasData = evolution.some((m) => m.expense > 0);

  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Evolución de gastos (últimos 6 meses)</Typography>
      {!hasData ? (
        <Typography color="text.secondary">Sin movimientos en el período.</Typography>
      ) : (
        <BarChart
          height={280}
          xAxis={[{ scaleType: "band", data: evolution.map((m) => MONTH_LABELS[m.month - 1]) }]}
          series={[{ data: evolution.map((m) => m.expense), label: `Gastos (${currency})` }]}
        />
      )}
    </Box>
  );
}
