"use client";

import { Box, Button, Container, FormControl, InputLabel, MenuItem, Select, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../../../lib/auth";

type Account = { id: string; name: string; currency: string };
type Category = { id: string; name: string; type: number };
type Movement = { id: string; accountId: string; categoryName?: string; type: number; amount: number; date: string; description?: string };

export default function NuevoMovimientoPage() {
  const router = useRouter();
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [movements, setMovements] = useState<Movement[]>([]);
  const [accountId, setAccountId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [amount, setAmount] = useState("");
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10));
  const [description, setDescription] = useState("");
  const [error, setError] = useState("");
  const [ok, setOk] = useState("");

  const load = async () => {
    const [acc, cat, mov] = await Promise.all([
      apiFetch("/api/accounts"),
      apiFetch("/api/categories?type=1"),
      apiFetch("/api/movements"),
    ]);
    if (acc.status === 401) { router.push("/login"); return; }
    if (acc.ok) setAccounts(await acc.json());
    if (cat.ok) setCategories(await cat.json());
    if (mov.ok) setMovements(await mov.json());
  };

  useEffect(() => {
    if (!getToken()) router.push("/login");
    else load();
  }, [router]);

  const handleCreate = async () => {
    setError(""); setOk("");
    const res = await apiFetch("/api/movements", {
      method: "POST",
      body: JSON.stringify({
        accountId,
        categoryId: categoryId || null,
        type: 1,
        amount: Number(amount),
        date: new Date(date).toISOString(),
        description: description || null,
      }),
    });
    if (res.ok) {
      setOk("Ingreso registrado.");
      setAmount(""); setDescription(""); setCategoryId("");
      load();
    } else if (res.status === 400) {
      setError((await res.json()).mensaje || "Datos inválidos.");
    } else if (res.status === 404) {
      setError("Cuenta no encontrada.");
    } else if (res.status === 401) {
      router.push("/login");
    } else {
      setError("No se pudo registrar el ingreso.");
    }
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 4, display: "flex", flexDirection: "column", gap: 2 }}>
        <Typography variant="h5">Nuevo ingreso</Typography>

        <FormControl fullWidth>
          <InputLabel>Cuenta</InputLabel>
          <Select value={accountId} label="Cuenta" onChange={(e) => setAccountId(e.target.value)}>
            {accounts.map((a) => <MenuItem key={a.id} value={a.id}>{a.name} ({a.currency})</MenuItem>)}
          </Select>
        </FormControl>

        <TextField label="Monto" type="number" value={amount} onChange={(e) => setAmount(e.target.value)} fullWidth />
        <TextField label="Fecha" type="date" value={date} onChange={(e) => setDate(e.target.value)} fullWidth slotProps={{ inputLabel: { shrink: true } }} />

        <FormControl fullWidth>
          <InputLabel>Categoría (opcional)</InputLabel>
          <Select value={categoryId} label="Categoría (opcional)" onChange={(e) => setCategoryId(e.target.value)}>
            <MenuItem value="">Sin categoría</MenuItem>
            {categories.map((c) => <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>)}
          </Select>
        </FormControl>

        <TextField label="Descripción (opcional)" value={description} onChange={(e) => setDescription(e.target.value)} fullWidth />

        {error && <Typography color="error">{error}</Typography>}
        {ok && <Typography color="success.main">{ok}</Typography>}

        <Button variant="contained" onClick={handleCreate}>Registrar ingreso</Button>

        <Typography variant="h6" sx={{ mt: 2 }}>Movimientos</Typography>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
          {movements.map((m) => (
            <Box key={m.id} sx={{ p: 1, border: 1, borderColor: "divider", borderRadius: 1 }}>
              <Typography variant="body2">{m.type === 1 ? "Ingreso" : "Gasto"} · {m.amount.toFixed(2)}</Typography>
              <Typography variant="caption" color="text.secondary">{new Date(m.date).toLocaleDateString()} · {m.categoryName ?? "Sin categoría"} {m.description ? `· ${m.description}` : ""}</Typography>
            </Box>
          ))}
          {movements.length === 0 && <Typography color="text.secondary">No hay movimientos todavía.</Typography>}
        </Box>
      </Box>
    </Container>
  );
}
