"use client";

import { Box, Button, Container, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";

type Message = { role: "user" | "assistant"; text: string };

function RichText({ text }: { text: string }) {
  const parts = text.split("**");
  return (
    <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>
      {parts.map((p, i) =>
        i % 2 === 1 ? <b key={i}>{p}</b> : <span key={i}>{p}</span>
      )}
    </Typography>
  );
}

function TypingDots() {
  return (
    <Box sx={{ display: "flex", gap: 0.5, p: 1.5, alignSelf: "flex-start", bgcolor: "background.paper", border: 1, borderColor: "divider", borderRadius: 2 }}>
      {[0, 1, 2].map((i) => (
        <Box
          key={i}
          sx={{
            width: 8, height: 8, borderRadius: "50%", bgcolor: "primary.main",
            animation: "typing 1.2s infinite",
            animationDelay: `${i * 0.2}s`,
            "@keyframes typing": {
              "0%, 60%, 100%": { opacity: 0.25, transform: "translateY(0)" },
              "30%": { opacity: 1, transform: "translateY(-4px)" },
            },
          }}
        />
      ))}
    </Box>
  );
}

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
                ...(m.role === "user"
                  ? { bgcolor: "primary.main", color: "#fff" }
                  : { bgcolor: "background.paper", border: 1, borderColor: "divider" }),
              }}
            >
              <RichText text={m.text} />
            </Box>
          ))}
          {loading && <TypingDots />}
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
