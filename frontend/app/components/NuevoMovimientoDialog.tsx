"use client";

import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, InputLabel, MenuItem, Select, TextField } from "@mui/material";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { apiFetch } from "../lib/auth";

type Account = { id: string; name: string; currency: string };
type Category = { id: string; name: string; type: number };

const TIPO_INGRESO = 1;
const TIPO_GASTO = 2;

export default function NuevoMovimientoDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
  const router = useRouter();
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [type, setType] = useState<number>(TIPO_INGRESO);
  const [accountId, setAccountId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [amount, setAmount] = useState("");
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10));
  const [description, setDescription] = useState("");
  const [error, setError] = useState("");
  const [ok, setOk] = useState("");
  const [sending, setSending] = useState(false);

  const loadAccounts = useCallback(async () => {
    const res = await apiFetch("/api/accounts");
    if (res.status === 401) { router.push("/login"); return; }
    if (res.ok) setAccounts(await res.json());
  }, [router]);

  const loadCategories = useCallback(async (t: number) => {
    const res = await apiFetch(`/api/categories?type=${t}`);
    if (res.ok) setCategories(await res.json());
  }, []);

  // Al abrir: cargar cuentas y limpiar el formulario.
  useEffect(() => {
    if (!open) return;
    let cancelled = false;
    Promise.resolve().then(() => {
      if (cancelled) return;
      setType(TIPO_INGRESO);
      setAccountId(""); setCategoryId(""); setAmount(""); setDescription("");
      setDate(new Date().toISOString().slice(0, 10));
      setError(""); setOk("");
      loadAccounts();
    });
    return () => { cancelled = true; };
  }, [open, loadAccounts]);

  // Al cambiar el tipo: recargar categorías y limpiar la elegida.
  useEffect(() => {
    if (!open) return;
    let cancelled = false;
    Promise.resolve().then(() => {
      if (cancelled) return;
      setCategoryId("");
      loadCategories(type);
    });
    return () => { cancelled = true; };
  }, [open, type, loadCategories]);

  const esGasto = type === TIPO_GASTO;

  const handleCreate = async () => {
    setError(""); setOk("");
    if (esGasto && !description.trim()) {
      setError("En los gastos la descripción es obligatoria.");
      return;
    }
    setSending(true);
    try {
      const res = await apiFetch("/api/movements", {
        method: "POST",
        body: JSON.stringify({
          accountId,
          categoryId: categoryId || null,
          type,
          amount: Number(amount),
          date: new Date(date).toISOString(),
          description: description.trim() || null,
        }),
      });
      if (res.ok) {
        setOk(esGasto ? "Gasto registrado." : "Ingreso registrado.");
        setAmount(""); setDescription(""); setCategoryId("");
        window.dispatchEvent(new CustomEvent("finova:movement-created"));
      } else if (res.status === 400) {
        setError((await res.json()).mensaje || "Datos inválidos.");
      } else if (res.status === 404) {
        setError("Cuenta no encontrada.");
      } else if (res.status === 401) {
        router.push("/login");
      } else {
        setError("No se pudo registrar el movimiento.");
      }
    } finally {
      setSending(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Nuevo movimiento</DialogTitle>
      <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: "12px !important" }}>
        <FormControl fullWidth>
          <InputLabel>Tipo</InputLabel>
          <Select value={type} label="Tipo" onChange={(e) => setType(Number(e.target.value))}>
            <MenuItem value={TIPO_INGRESO}>Ingreso</MenuItem>
            <MenuItem value={TIPO_GASTO}>Gasto</MenuItem>
          </Select>
        </FormControl>

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

        <TextField
          label={esGasto ? "Descripción (obligatoria)" : "Descripción (opcional)"}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          fullWidth
          required={esGasto}
        />

        {error && <Alert severity="error">{error}</Alert>}
        {ok && <Alert severity="success">{ok}</Alert>}
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={onClose} color="inherit">Cerrar</Button>
        <Button
          variant="contained"
          onClick={handleCreate}
          disabled={sending || !accountId || !amount || Number(amount) <= 0}
        >
          {esGasto ? "Registrar gasto" : "Registrar ingreso"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
