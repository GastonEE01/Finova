export const API_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:5171";

export function getToken() {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("finova_token");
}

export function setToken(token: string, email: string) {
  localStorage.setItem("finova_token", token);
  localStorage.setItem("finova_email", email);
}

export function clearToken() {
  localStorage.removeItem("finova_token");
  localStorage.removeItem("finova_email");
}

export function getEmail() {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("finova_email");
}

export async function apiFetch(path: string, options: RequestInit = {}) {
  const token = getToken();
  const headers: Record<string, string> = { "Content-Type": "application/json", ...(options.headers as Record<string, string>) };
  if (token) headers["Authorization"] = `Bearer ${token}`;
  const res = await fetch(`${API_URL}${path}`, { ...options, headers });
  return res;
}
