"use client";

import { Box, Button, Container, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";

type Message = { role: "user" | "assistant"; text: string };

const SUGERENCIAS = [
  "¿cuánto gasté este mes?",
  "¿en qué gasté más?",
  "¿cuánto tengo en total?",
  "¿gasté más que el mes pasado?",
  "¿cómo reduzco mis gastos?",
];

export default function AsistentePage() {
  const router = useRouter();
  const [messages, setMessages] = useState<Message[]>([]);
  const [question, setQuestion] = useState("");
  const [loading, setLoading] = useState(false);

  const enviar = async (texto?: string) => {
    const q = (texto ?? question).trim();
    if (!q || loading) return;
    if (!getToken()) { router.push("/login"); return; }
    setMessages((prev) => [...prev, { role: "user", text: q }]);
    setQuestion("");
    setLoading(true);
    try {
      const res = await apiFetch("/api/assistant/chat", {
        method: "POST",
        body: JSON.stringify({ question: q }),
      });
      if (res.status === 401) { router.push("/login"); return; }
      if (res.status === 503) {
        setMessages((prev) => [...prev, { role: "assistant", text: "Asistente no disponible, intentá más tarde" }]);
        return;
      }
      if (!res.ok) {
        const data = await res.json().catch(() => null);
        const msg = typeof data === "string" ? data : "No se pudo obtener la respuesta.";
        setMessages((prev) => [...prev, { role: "assistant", text: msg }]);
        return;
      }
      const data = await res.json();
      setMessages((prev) => [...prev, { role: "assistant", text: data.answer ?? "Sin respuesta." }]);
    } catch {
      setMessages((prev) => [...prev, { role: "assistant", text: "Asistente no disponible, intentá más tarde" }]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 4, display: "flex", flexDirection: "column", gap: 2, pb: 4 }}>
        <Typography variant="h5">Asistente financiero</Typography>
        <Typography variant="body2" color="text.secondary">
          Preguntá sobre tus finanzas. Las respuestas usan solo tus datos reales.
        </Typography>

        <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
          {SUGERENCIAS.map((s) => (
            <Button key={s} variant="outlined" size="small" onClick={() => enviar(s)} disabled={loading}>
              {s}
            </Button>
          ))}
        </Box>

        <Box sx={{ display: "flex", flexDirection: "column", gap: 1, minHeight: 120 }}>
          {messages.length === 0 && !loading && (
            <Typography color="text.secondary">Todavía no hay mensajes. Hacé tu primera pregunta.</Typography>
          )}
          {messages.map((m, i) => (
            <Box
              key={i}
              sx={{
                alignSelf: m.role === "user" ? "flex-end" : "flex-start",
                maxWidth: "85%",
                p: 1.5,
                borderRadius: 2,
                bgcolor: m.role === "user" ? "primary.main" : "#f0f0f0",
                color: m.role === "user" ? "white" : "inherit",
              }}
            >
              <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>{m.text}</Typography>
            </Box>
          ))}
          {loading && <Typography color="text.secondary">Pensando…</Typography>}
        </Box>

        <Box sx={{ display: "flex", gap: 1 }}>
          <TextField
            fullWidth
            label="Escribí tu pregunta"
            value={question}
            onChange={(e) => setQuestion(e.target.value)}
            onKeyDown={(e) => { if (e.key === "Enter") enviar(); }}
            slotProps={{ htmlInput: { maxLength: 500 } }}
          />
          <Button variant="contained" onClick={() => enviar()} disabled={loading || !question.trim()}>
            Enviar
          </Button>
        </Box>

      </Box>
    </Container>
  );
}
