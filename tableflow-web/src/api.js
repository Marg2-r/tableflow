export const UNAUTHORIZED_EVENT = "tableflow:unauthorized";

export async function apiFetch(input, options = {}) {
  const response = await fetch(input, {
    ...options,
    credentials: "include",
  });

  if (response.status === 401) {
    window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
  }

  return response;
}
