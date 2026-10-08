"use client";

import { Box, Button, Chip, Container, Dialog, DialogActions, DialogContent, DialogTitle, LinearProgress, MenuItem, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";

type Budget = { id: string; categoryId: string; categoryName: string; year: number; month: number; amount: number; currency: string; spent: number; remaining: number; percent: number; status: string };
type Category = { id: string; name: string; type: number };

const estadoLabel: Record<string, string> = { Ok: "OK", Acercandose: "Acercándose", Superado: "Superado" };
const estadoColor: Record<string, "success" | "warning" | "error"> = { Ok: "success", Acercandose: "warning", Superado: "error" };

export default function PresupuestosPage() {
  const router = useRouter();
  const now = new Date();
  const [year, setYear] = useState(now.getUTCFullYear());
  const [month, setMonth] = useState(now.getUTCMonth() + 1);
  const [budgets, setBudgets] = useState<Budget[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Budget | null>(null);
  const [categoryId, setCategoryId] = useState("");
  const [amount, setAmount] = useState("");
  const [currency, setCurrency] = useState("ARS");
  const [error, setError] = useState("");

  const load = useCallback(async (y: number, m: number) => {
    const res = await apiFetch(`/api/budgets?year=${y}&month=${m}`);
    if (res.status === 401) { router.push("/login"); return; }
    if (res.ok) setBudgets(await res.json());
  }, [router]);

  const loadCategories = useCallback(async () => {
    const res = await apiFetch("/api/categories?type=2");
    if (res.ok) setCategories(await res.json());
  }, []);

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    let cancelled = false;
    Promise.resolve().then(() => {
      if (cancelled) return;
      load(year, month);
      loadCategories();
    });
    return () => { cancelled = true; };
  }, [router, load, loadCategories, year, month]);

  const refresh = () => load(year, month);

  const openCreate = () => {
    setEditing(null); setCategoryId(""); setAmount(""); setCurrency("ARS"); setError(""); setOpen(true);
  };

  const openEdit = (b: Budget) => {
    setEditing(b); setAmount(String(b.amount)); setError(""); setOpen(true);
  };

  const handleSave = async () => {
    setError("");
    const monto = Number(amount);
    if (!monto || monto <= 0) { setError("El monto debe ser mayor a 0."); return; }
    let res;
    if (editing) {
      res = await apiFetch(`/api/budgets/${editing.id}`, { method: "PUT", body: JSON.stringify({ amount: monto }) });
    } else {
      if (!categoryId) { setError("Elegí una categoría."); return; }
      res = await apiFetch("/api/budgets", { method: "POST", body: JSON.stringify({ categoryId, year, month, amount: monto, currency }) });
    }
    if (res.ok) { setOpen(false); refresh(); }
    else {
      try { const data = await res.json(); setError(data.mensaje ?? "No se pudo guardar el presupuesto."); }
      catch { setError("No se pudo guardar el presupuesto."); }
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("¿Eliminar este presupuesto?")) return;
    const res = await apiFetch(`/api/budgets/${id}`, { method: "DELETE" });
    if (res.ok) refresh();
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 4, display: "flex", flexDirection: "column", gap: 2 }}>
        <Typography variant="h5">Presupuestos</Typography>
        <Box sx={{ display: "flex", gap: 2 }}>
          <TextField label="Año" type="number" value={year} onChange={(e) => { const y = Number(e.target.value); setYear(y); load(y, month); }} fullWidth />
          <TextField label="Mes" type="number" value={month} slotProps={{ htmlInput: { min: 1, max: 12 } }} onChange={(e) => { const m = Number(e.target.value); setMonth(m); load(year, m); }} fullWidth />
        </Box>
        <Button variant="contained" onClick={openCreate}>Nuevo presupuesto</Button>
        {budgets.map((b) => (
          <Box key={b.id} sx={{ border: 1, borderColor: "divider", borderRadius: 2, p: 2, display: "flex", flexDirection: "column", gap: 1 }}>
            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 1 }}>
              <Typography variant="subtitle1">{b.categoryName}</Typography>
              <Chip label={estadoLabel[b.status] ?? b.status} color={estadoColor[b.status] ?? "default"} size="small" />
            </Box>
            <Typography variant="body2" color="text.secondary">
              Gastado {b.spent.toFixed(2)} / Límite {b.amount.toFixed(2)} {b.currency} · Restante {b.remaining.toFixed(2)}
            </Typography>
            <LinearProgress variant="determinate" value={Math.min(b.percent, 100)} color={estadoColor[b.status] ?? "primary"} />
            <Typography variant="caption" color="text.secondary">{b.percent.toFixed(1)}% del presupuesto</Typography>
            <Box sx={{ display: "flex", gap: 1 }}>
              <Button size="small" onClick={() => openEdit(b)}>Editar monto</Button>
              <Button size="small" color="error" onClick={() => handleDelete(b.id)}>Eliminar</Button>
            </Box>
          </Box>
        ))}
        {budgets.length === 0 && <Typography color="text.secondary">No hay presupuestos para este mes.</Typography>}
      </Box>

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth>
        <DialogTitle>{editing ? "Editar presupuesto" : "Nuevo presupuesto"}</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 2 }}>
          {!editing && (
            <TextField select label="Categoría de gasto" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} fullWidth>
              {categories.map((c) => <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>)}
            </TextField>
          )}
          <TextField label="Monto límite" type="number" value={amount} onChange={(e) => setAmount(e.target.value)} fullWidth />
          {!editing && (
            <TextField label="Moneda (ej. ARS)" value={currency} onChange={(e) => setCurrency(e.target.value)} fullWidth />
          )}
          {error && <Typography color="error">{error}</Typography>}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={handleSave}>Guardar</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}
