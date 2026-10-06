"use client";

import { Box, Button, Container, FormControl, InputLabel, MenuItem, Select, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../lib/auth";
import BalanceEvolutionChart from "./balance-evolution-chart";
import ComparisonsSection from "./comparisons-section";
import ExpensesByCategoryChart from "./expenses-by-category-chart";
import IncomeVsExpensesChart from "./income-vs-expenses-chart";

type CurrencyTotal = { currency: string; amount: number };
type RecentItem = { id: string; accountName: string; currency: string; categoryName?: string; type: number; amount: number; date: string; description?: string };
type Dashboard = { totalBalances: CurrencyTotal[]; totalIncome: CurrencyTotal[]; totalExpenses: CurrencyTotal[]; recentMovements: RecentItem[] };

function Totals({ title, items, empty }: { title: string; items: CurrencyTotal[]; empty: string }) {
  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>{title}</Typography>
      {items.length === 0
        ? <Typography color="text.secondary">{empty}</Typography>
        : items.map((t) => (
          <Typography key={t.currency} variant="body1">
            {t.amount.toFixed(2)} {t.currency}
          </Typography>
        ))}
    </Box>
  );
}

export default function DashboardPage() {
  const router = useRouter();
  const [data, setData] = useState<Dashboard | null>(null);
  const [error, setError] = useState("");
  const [currency, setCurrency] = useState("");

  const currencies = data
    ? [...new Set([
      ...data.totalBalances.map((t) => t.currency),
      ...data.totalIncome.map((t) => t.currency),
      ...data.totalExpenses.map((t) => t.currency),
    ])]
    : [];
  const selectedCurrency = currency || currencies[0] || "";

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    apiFetch("/api/dashboard")
      .then(async (res) => {
        if (res.status === 401) { router.push("/login"); return; }
        if (res.ok) setData(await res.json());
        else setError("No se pudo cargar el panel.");
      })
      .catch(() => setError("No se pudo cargar el panel."));
  }, [router]);

  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 4, display: "flex", flexDirection: "column", gap: 3 }}>
        <Typography variant="h5">Panel</Typography>

        {error && <Typography color="error">{error}</Typography>}
        {!error && !data && <Typography color="text.secondary">Cargando...</Typography>}

        {data && (
          <>
            <Totals title="Saldo total" items={data.totalBalances} empty="Sin cuentas todavía." />
            <Totals title="Ingresos del mes" items={data.totalIncome} empty="Sin ingresos este mes." />
            <Totals title="Gastos del mes" items={data.totalExpenses} empty="Sin gastos este mes." />

            <Box>
              <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Últimos movimientos</Typography>
              <Box sx={{ display: "flex", flexDirection: "column", gap: 1, mt: 1 }}>
                {data.recentMovements.map((m) => (
                  <Box key={m.id} sx={{ p: 1.5, border: "1px solid #ddd", borderRadius: 1 }}>
                    <Typography variant="body2" sx={{ fontWeight: "bold" }}>
                      {m.type === 1 ? "Ingreso" : "Gasto"} · {m.amount.toFixed(2)} {m.currency}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {new Date(m.date).toLocaleDateString()} · {m.accountName} · {m.categoryName ?? "Sin categoría"}{m.description ? ` · ${m.description}` : ""}
                    </Typography>
                  </Box>
                ))}
                {data.recentMovements.length === 0 && (
                  <Typography color="text.secondary">No hay movimientos.</Typography>
                )}
              </Box>
            </Box>

            <Box sx={{ display: "flex", gap: 2 }}>
              <Button variant="contained" href="/movimientos">Ver historial</Button>
              <Button variant="outlined" href="/cuentas">Ver cuentas</Button>
            </Box>

            <Box>
              <Typography variant="h6">Gráficos</Typography>
              {currencies.length === 0 && (
                <Typography color="text.secondary">Sin datos para mostrar gráficos.</Typography>
              )}
              {currencies.length > 1 && (
                <FormControl fullWidth sx={{ mt: 1, mb: 2 }}>
                  <InputLabel id="currency-label">Moneda</InputLabel>
                  <Select
                    labelId="currency-label"
                    label="Moneda"
                    value={selectedCurrency}
                    onChange={(e) => setCurrency(e.target.value)}
                  >
                    {currencies.map((c) => (
                      <MenuItem key={c} value={c}>{c}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}
              {selectedCurrency && (
                <Box sx={{ display: "flex", flexDirection: "column", gap: 3, mt: 1 }}>
                  <ExpensesByCategoryChart currency={selectedCurrency} />
                  <IncomeVsExpensesChart currency={selectedCurrency} />
                  <BalanceEvolutionChart currency={selectedCurrency} />
                  <ComparisonsSection currency={selectedCurrency} />
                </Box>
              )}
            </Box>
          </>
        )}

        <Button href="/">Volver</Button>
      </Box>
    </Container>
  );
}
