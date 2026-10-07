"use client";

import { Box, Typography } from "@mui/material";
import { BarChart } from "@mui/x-charts";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";
import { useChartPalette, fmtMoney } from "../../components/chartPalette";

const MONTH_LABELS = ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];

type MonthItem = { year: number; month: number; income: number; expense: number };
type Response = { currency: string; months: MonthItem[] };

export default function IncomeVsExpensesChart({ currency }: { currency: string }) {
  const router = useRouter();
  const palette = useChartPalette();
  const [data, setData] = useState<Response | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    setData(null);
    setError("");
    apiFetch(`/api/dashboard/income-vs-expenses?currency=${encodeURIComponent(currency)}`)
      .then(async (res) => {
        if (res.status === 401) { router.push("/login"); return; }
        if (res.ok) setData(await res.json());
        else setError("No se pudo cargar el gráfico.");
      })
      .catch(() => setError("No se pudo cargar el gráfico."));
  }, [currency, router]);

  const hasData = data && data.months.some((m) => m.income > 0 || m.expense > 0);

  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Ingresos vs. gastos (últimos 6 meses)</Typography>
      {error && <Typography color="error">{error}</Typography>}
      {!error && !data && <Typography color="text.secondary">Cargando gráfico...</Typography>}
      {data && !hasData && (
        <Typography color="text.secondary">Sin movimientos en el período.</Typography>
      )}
      {data && hasData && (
        <BarChart
          height={280}
          borderRadius={6}
          xAxis={[{ scaleType: "band", data: data.months.map((m) => MONTH_LABELS[m.month - 1]) }]}
          series={[
            { data: data.months.map((m) => m.income), label: "Ingresos", color: palette.income, valueFormatter: fmtMoney(currency) },
            { data: data.months.map((m) => m.expense), label: "Gastos", color: palette.expense, valueFormatter: fmtMoney(currency) },
          ]}
        />
      )}
    </Box>
  );
}
