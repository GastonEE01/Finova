"use client";

import { IconButton, InputAdornment, TextField, type TextFieldProps } from "@mui/material";
import { useState } from "react";
import { FiEye, FiEyeOff } from "react-icons/fi";

// TextField de contraseña con botón de ojo para mostrar/ocultar lo que se escribe.
export default function PasswordField(props: TextFieldProps) {
  const [visible, setVisible] = useState(false);

  return (
    <TextField
      {...props}
      type={visible ? "text" : "password"}
      slotProps={{
        ...props.slotProps,
        input: {
          ...props.slotProps?.input,
          endAdornment: (
            <InputAdornment position="end">
              <IconButton
                aria-label={visible ? "Ocultar contraseña" : "Mostrar contraseña"}
                onClick={() => setVisible((v) => !v)}
                edge="end"
                size="small"
                sx={{ color: "text.secondary", "&:hover": { bgcolor: "transparent", color: "#FFFFFF" } }}
              >
                {visible ? <FiEyeOff size={20} /> : <FiEye size={20} />}
              </IconButton>
            </InputAdornment>
          ),
        },
      }}
    />
  );
}
