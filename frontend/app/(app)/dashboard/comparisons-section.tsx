"use client";

import { Box, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";
import ExpenseEvolutionChart from "./expense-evolution-chart";
import TopCategoriesChart, { TopCategory } from "./top-categories-chart";

type MonthTotal = { year: number; month: number; income: number; expense: number };
type EvolutionItem = { year: number; month: number; expense: number };
type Comparisons = {
  currency: string;
  currentMonth: MonthTotal;
  previousMonth: MonthTotal;
  incomeVariationPct: number | null;
  expenseVariationPct: number | null;
  topCategories: TopCategory[];
  expenseEvolution: EvolutionItem[];
};

function VariationText({ value }: { value: number | null }) {
  if (value === null || value === undefined)
    return <Typography variant="body2" color="text.secondary">— sin datos previos</Typography>;
  const color = value > 0 ? "success.main" : value < 0 ? "error.main" : "text.secondary";
  const sign = value > 0 ? "+" : "";
  const label = value === 0 ? "sin cambios" : `${sign}${value.toFixed(2)}% vs. mes anterior`;
  return <Typography variant="body2" sx={{ color }}>{value === 0 ? label : label}</Typography>;
}

function MonthCard({ title, current, previous, variation, currency }: {
  title: string; current: number; previous: number; variation: number | null; currency: string;
}) {
  return (
    <Box sx={{ p: 1.5, border: 1, borderColor: "divider", borderRadius: 1, flex: 1 }}>
      <Typography variant="subtitle2" sx={{ fontWeight: "bold" }}>{title}</Typography>
      <Typography variant="body2">Este mes: {current.toFixed(2)} {currency}</Typography>
      <Typography variant="body2" color="text.secondary">Anterior: {previous.toFixed(2)} {currency}</Typography>
      <VariationText value={variation} />
    </Box>
  );
}

export default function ComparisonsSection({ currency }: { currency: string }) {
  const router = useRouter();
  const [data, setData] = useState<Comparisons | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    setData(null);
    setError("");
    apiFetch(`/api/dashboard/comparisons?currency=${encodeURIComponent(currency)}`)
      .then(async (res) => {
        if (res.status === 401) { router.push("/login"); return; }
        if (res.ok) setData(await res.json());
        else if (res.status === 400) setError("Debe indicar la moneda.");
        else setError("No se pudieron cargar las comparaciones.");
      })
      .catch(() => setError("No se pudieron cargar las comparaciones."));
  }, [currency, router]);

  return (
    <Box>
      <Typography variant="h6">Comparaciones</Typography>
      {error && <Typography color="error">{error}</Typography>}
      {!error && !data && <Typography color="text.secondary">Cargando comparaciones...</Typography>}
      {data && (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 3, mt: 1 }}>
          <Box>
            <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Este mes vs. mes anterior</Typography>
            <Box sx={{ display: "flex", flexDirection: { xs: "column", sm: "row" }, gap: 1.5, mt: 1 }}>
              <MonthCard
                title="Ingresos"
                current={data.currentMonth.income}
                previous={data.previousMonth.income}
                variation={data.incomeVariationPct}
                currency={currency}
              />
              <MonthCard
                title="Gastos"
                current={data.currentMonth.expense}
                previous={data.previousMonth.expense}
                variation={data.expenseVariationPct}
                currency={currency}
              />
            </Box>
          </Box>
          <TopCategoriesChart items={data.topCategories} currency={currency} />
          <ExpenseEvolutionChart evolution={data.expenseEvolution} currency={currency} />
        </Box>
      )}
    </Box>
  );
}
