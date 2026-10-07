"use client";

import { Box, Button, Container, FormControl, InputLabel, MenuItem, Select, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";

type Account = { id: string; name: string; currency: string };
type Category = { id: string; name: string; type: number };
type HistoryItem = { id: string; accountId: string; accountName: string; currency: string; categoryId?: string; categoryName?: string; type: number; amount: number; date: string; description?: string; runningBalance: number };

export default function MovimientosPage() {
  const router = useRouter();
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [items, setItems] = useState<HistoryItem[]>([]);
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [accountId, setAccountId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [type, setType] = useState("");
  const [error, setError] = useState("");

  const loadFilters = useCallback(async () => {
    const [acc, cat] = await Promise.all([apiFetch("/api/accounts"), apiFetch("/api/categories")]);
    if (acc.status === 401) { router.push("/login"); return; }
    if (acc.ok) setAccounts(await acc.json());
    if (cat.ok) setCategories(await cat.json());
  }, [router]);

  const loadHistory = useCallback(async () => {
    setError("");
    const params = new URLSearchParams();
    if (from) params.set("from", new Date(from).toISOString());
    if (to) params.set("to", new Date(to).toISOString());
    if (accountId) params.set("accountId", accountId);
    if (categoryId) params.set("categoryId", categoryId);
    if (type) params.set("type", type);
    const res = await apiFetch(`/api/movements/history?${params}`);
    if (res.status === 401) { router.push("/login"); return; }
    if (res.ok) setItems(await res.json());
    else if (res.status === 400) setError((await res.json()).mensaje || "Filtros inválidos.");
    else setError("No se pudo cargar el historial.");
  }, [from, to, accountId, categoryId, type, router]);

  useEffect(() => {
    if (!getToken()) router.push("/login");
    else { loadFilters(); loadHistory(); }
  }, [router, loadFilters, loadHistory]);

  const clearFilters = () => {
    setFrom(""); setTo(""); setAccountId(""); setCategoryId(""); setType("");
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 4, display: "flex", flexDirection: "column", gap: 2 }}>
        <Typography variant="h5">Historial de movimientos</Typography>

        <Box sx={{ display: "flex", gap: 2 }}>
          <TextField label="Desde" type="date" value={from} onChange={(e) => setFrom(e.target.value)} fullWidth slotProps={{ inputLabel: { shrink: true } }} />
          <TextField label="Hasta" type="date" value={to} onChange={(e) => setTo(e.target.value)} fullWidth slotProps={{ inputLabel: { shrink: true } }} />
        </Box>

        <FormControl fullWidth>
          <InputLabel>Cuenta</InputLabel>
          <Select value={accountId} label="Cuenta" onChange={(e) => setAccountId(e.target.value)}>
            <MenuItem value="">Todas</MenuItem>
            {accounts.map((a) => <MenuItem key={a.id} value={a.id}>{a.name} ({a.currency})</MenuItem>)}
          </Select>
        </FormControl>

        <FormControl fullWidth>
          <InputLabel>Categoría</InputLabel>
          <Select value={categoryId} label="Categoría" onChange={(e) => setCategoryId(e.target.value)}>
            <MenuItem value="">Todas</MenuItem>
            {categories.map((c) => <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>)}
          </Select>
        </FormControl>

        <FormControl fullWidth>
          <InputLabel>Tipo</InputLabel>
          <Select value={type} label="Tipo" onChange={(e) => setType(e.target.value)}>
            <MenuItem value="">Todos</MenuItem>
            <MenuItem value="1">Ingresos</MenuItem>
            <MenuItem value="2">Gastos</MenuItem>
          </Select>
        </FormControl>

        <Box sx={{ display: "flex", gap: 2 }}>
          <Button variant="contained" onClick={loadHistory}>Filtrar</Button>
          <Button onClick={clearFilters}>Limpiar</Button>
        </Box>

        {error && <Typography color="error">{error}</Typography>}

        <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
          {items.map((m) => (
            <Box key={m.id} sx={{ p: 1.5, border: 1, borderColor: "divider", borderRadius: 1 }}>
              <Typography variant="body2" sx={{ fontWeight: "bold" }}>
                {m.type === 1 ? "Ingreso" : "Gasto"} · {m.amount.toFixed(2)} {m.currency}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {new Date(m.date).toLocaleDateString()} · {m.accountName} · {m.categoryName ?? "Sin categoría"}{m.description ? ` · ${m.description}` : ""}
              </Typography>
              <Typography variant="caption" color="text.secondary" sx={{ display: "block" }}>
                Saldo: {m.runningBalance.toFixed(2)} {m.currency}
              </Typography>
            </Box>
          ))}
          {items.length === 0 && <Typography color="text.secondary">No hay movimientos.</Typography>}
        </Box>

      </Box>
    </Container>
  );
}
