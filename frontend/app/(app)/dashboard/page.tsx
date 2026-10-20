"use client";

import { Box, Container, FormControl, InputLabel, MenuItem, Select, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";
import BalanceEvolutionChart from "./balance-evolution-chart";
import ComparisonsSection from "./comparisons-section";
import ExpensesByCategoryChart from "./expenses-by-category-chart";
import IncomeVsExpensesChart from "./income-vs-expenses-chart";

type CurrencyTotal = { currency: string; amount: number };
type RecentItem = { id: string; accountName: string; currency: string; categoryName?: string; type: number; amount: number; date: string; description?: string };
type Dashboard = { totalBalances: CurrencyTotal[]; totalIncome: CurrencyTotal[]; totalExpenses: CurrencyTotal[]; recentMovements: RecentItem[] };

const MONTH_NAMES = ["enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];

function monthOptions(count: number) {
  const now = new Date();
  return Array.from({ length: count }, (_, i) => {
    const d = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth() - i, 1));
    return { year: d.getUTCFullYear(), month: d.getUTCMonth() + 1, label: `${MONTH_NAMES[d.getUTCMonth()]} ${d.getUTCFullYear()}` };
  });
}

function Card({ children }: { children: React.ReactNode }) {
  return (
    <Box sx={{ p: 2, border: "1px solid", borderColor: "divider", borderRadius: 3, bgcolor: "background.paper", minWidth: 0 }}>
      {children}
    </Box>
  );
}

function Totals({
  title,
  items,
  empty,
}: {
  title: string;
  items: CurrencyTotal[];
  empty: string;
}) {
  return (
    <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
      {items.length === 0 ? (
        <Typography variant="body2" color="textPrimary">
          {empty}
        </Typography>
      ) : (
        items.map((t) => (
          <Typography
            key={t.currency}
            variant="h4"
            sx={{ fontWeight: 700 }}
          >
            {t.amount.toFixed(2)} {t.currency}
          </Typography>
        ))
      )}

      <Typography variant="h6" color="text.secondary">
        {title}
      </Typography>
    </Box>
  );
}


export default function DashboardPage() {
  const router = useRouter();
  const [data, setData] = useState<Dashboard | null>(null);
  const [error, setError] = useState("");
  const [currency, setCurrency] = useState("");
  const now = new Date();
  const [year, setYear] = useState(now.getUTCFullYear());
  const [month, setMonth] = useState(now.getUTCMonth() + 1);
  const months = useMemo(() => monthOptions(12), []);
  const selectedMonthLabel = months.find((m) => m.year === year && m.month === month)?.label ?? "";
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
    apiFetch(`/api/dashboard?year=${year}&month=${month}`)
      .then(async (res) => {
        if (res.status === 401) { router.push("/login"); return; }
        if (res.ok) setData(await res.json());
        else setError("No se pudo cargar el panel.");
      })
      .catch(() => setError("No se pudo cargar el panel."));
  }, [router, year, month]);

  return (
    <Container maxWidth={false}>
      <Box sx={{ mt: 4, display: "flex", flexDirection: "column", gap: 3 }}>
        <Box sx={{ display: "flex", flexDirection: { xs: "column", sm: "row" }, alignItems: { xs: "flex-start", sm: "center" }, justifyContent: "space-between", gap: 2, flexWrap: "wrap" }}>
          <Box>
            <Typography variant="h5" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700 }}>Panel</Typography>
            <Typography variant="h3" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, textTransform: "capitalize" }}>{selectedMonthLabel}</Typography>
          </Box>
          <FormControl size="small" sx={{ minWidth: 200 }}>
            <InputLabel id="month-select-label">Cambiar de mes</InputLabel>
            <Select
              labelId="month-select-label"
              label="Cambiar de mes"
              value={`${year}-${month}`}
              onChange={(e) => {
                const [y, m] = e.target.value.split("-").map(Number);
                setYear(y);
                setMonth(m);
              }}
            >
              {months.map((m) => (
                <MenuItem key={`${m.year}-${m.month}`} value={`${m.year}-${m.month}`}>{m.label}</MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        {error && <Typography color="error">{error}</Typography>}
        {!error && !data && <Typography color="text.secondary">Cargando...</Typography>}

        {data && (
          <>
          <Box sx={{ display: "flex", gap: 3, flexWrap: "wrap", padding: 2 }}>
            <Box sx={{ border: "1px solid gray", borderRadius: 2, p: 2, flex: "1 1 200px", minWidth: 0 }}>
            <Totals title="Total" items={data.totalBalances} empty="Sin cuentas todavía." />
            </Box>
            <Box sx={{ border: "1px solid gray", borderRadius: 2, p: 2, flex: "1 1 200px", minWidth: 0 }}>
            <Totals title={`Ingresos de ${selectedMonthLabel}`} items={data.totalIncome} empty="Sin ingresos este mes." />
            </Box>
            <Box sx={{ border: "1px solid gray", borderRadius: 2, p: 2, flex: "1 1 200px", minWidth: 0 }}>
            <Totals title={`Gastos de ${selectedMonthLabel}`} items={data.totalExpenses} empty="Sin gastos este mes." />
            </Box>
          </Box>

            <Box>
              <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", gap: 2, flexWrap: "wrap" }}>
                <Box>
                  <Typography variant="h5" sx={{ fontFamily: "Sora, sans-serif", color: "#73e2a1", backgroundColor: "#5e5959", p: 1, borderRadius: 4, width: "fit-content" }}>ANÁLISIS</Typography>
                  <Typography variant="h3" sx={{ fontWeight: "bold", marginTop: 2 }}>Dónde se te va la plata</Typography>
                </Box>
                {currencies.length > 1 && (
                  <FormControl size="small" sx={{ minWidth: 140 }}>
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
              </Box>
              {currencies.length === 0 && (
                <Typography color="text.secondary">Sin datos para mostrar gráficos.</Typography>
              )}
              {selectedCurrency && currencies.length > 0 && (
                <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" }, gap: 3, mt: 3 }}>
                  <Card><IncomeVsExpensesChart currency={selectedCurrency} year={year} month={month} /></Card>
                  <Card><ExpensesByCategoryChart currency={selectedCurrency} year={year} month={month} /></Card>
                  <Card><BalanceEvolutionChart currency={selectedCurrency} year={year} month={month} /></Card>
                  <Card><ComparisonsSection currency={selectedCurrency} year={year} month={month} /></Card>
                </Box>
              )}
            </Box>
          </>
        )}

      </Box>
    </Container>
  );
}
