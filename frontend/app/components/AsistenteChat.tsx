"use client";

import { Box, Button, Divider, Drawer, IconButton, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useRef, useState } from "react";
import { FiMessageCircle, FiX } from "react-icons/fi";
import { apiFetch, getToken } from "../lib/auth";

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

export function AsistenteFab({ onClick }: { onClick: () => void }) {
  return (
    <IconButton
      onClick={onClick}
      aria-label="Abrir asistente financiero"
      sx={{
        position: "fixed",
        right: { xs: 16, md: 24 },
        bottom: { xs: 80, md: 24 },
        zIndex: 1050,
        width: 56,
        height: 56,
        bgcolor: "primary.main",
        color: "#fff",
        boxShadow: 4,
        "&:hover": { bgcolor: "primary.dark" },
      }}
    >
      <FiMessageCircle size={26} />
    </IconButton>
  );
}

export default function AsistenteChat({ open, onClose }: { open: boolean; onClose: () => void }) {
  const router = useRouter();
  const [messages, setMessages] = useState<Message[]>([]);
  const [question, setQuestion] = useState("");
  const [loading, setLoading] = useState(false);
  const listRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    requestAnimationFrame(() => {
      listRef.current?.scrollTo({ top: listRef.current.scrollHeight, behavior: "smooth" });
    });
  };

  const enviar = async (texto?: string) => {
    const q = (texto ?? question).trim();
    if (!q || loading) return;
    if (!getToken()) { router.push("/login"); return; }
    setMessages((prev) => [...prev, { role: "user", text: q }]);
    setQuestion("");
    setLoading(true);
    scrollToBottom();
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
      scrollToBottom();
    }
  };

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      slotProps={{
        paper: {
          sx: {
            width: { xs: "100%", sm: 400 },
            borderTopLeftRadius: { xs: 0, sm: 3 },
            borderBottomLeftRadius: { xs: 0, sm: 3 },
          },
        },
      }}
    >
      <Box sx={{ display: "flex", alignItems: "center", gap: 1, p: 2 }}>
        <FiMessageCircle color="primary" size={22} />
        <Typography variant="h6" sx={{ flexGrow: 1 }}>Asistente financiero</Typography>
        <IconButton onClick={onClose} aria-label="Cerrar asistente" color="inherit">
          <FiX />
        </IconButton>
      </Box>
      <Divider />
      <Box sx={{ display: "flex", flexDirection: "column", gap: 2, p: 2, flexGrow: 1, overflowY: "auto" }}>
        <Typography variant="caption" color="text.secondary">
          Preguntá sobre tus finanzas. Las respuestas usan solo tus datos reales.
        </Typography>

        <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
          {SUGERENCIAS.map((s) => (
            <Button key={s} variant="outlined" size="small" onClick={() => enviar(s)} disabled={loading}>
              {s}
            </Button>
          ))}
        </Box>

        <Box ref={listRef} sx={{ display: "flex", flexDirection: "column", gap: 1, minHeight: 120, flexGrow: 1 }}>
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
            size="small"
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
    </Drawer>
  );
}
