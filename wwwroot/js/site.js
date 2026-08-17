async function handlePaymentFailure(orderId) {
    try {
        const response = await fetch(`/api/Payment/failed?orderId=${orderId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                // Add any other required headers here
            },
        });

        const result = await response.json();

        if (result.success) {
            // Redirect to the desired URL from the API response
            window.location.href = result.redirectUrl;
        } else {
            console.error('Failed to process payment');
        }
    } catch (error) {
        console.error('Error:', error);
    }
}
