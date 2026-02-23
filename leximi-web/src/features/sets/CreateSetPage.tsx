import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import { learningSetService, categoryService } from '../../api/services';
import { UpdateLearningSetDto, CreateLearningSetDto, UpdateLearningItemDto } from '../../types';
import SetEditor from './SetEditor';
import { useState } from 'react';

const CreateSetPage = () => {
    const navigate = useNavigate();
    const [categoryId, setCategoryId] = useState('');

    const { data: categories } = useQuery({
        queryKey: ['categories'],
        queryFn: categoryService.getAll
    });

    const createSetMutation = useMutation({
        mutationFn: async (data: UpdateLearningSetDto) => {
            // First create the set
            const createDto: CreateLearningSetDto = {
                title: data.title,
                description: data.description ?? undefined,
                type: data.type,
                categoryId: categoryId
            };
            const newSet = await learningSetService.create(createDto);

            // Then update with items if there are any
            if (data.items && data.items.length > 0) {
                await learningSetService.update(newSet.id, data);
            }
            return newSet;
        },
        onSuccess: (newSet) => {
            navigate(`/sets/${newSet.id}`);
        }
    });

    const handleSubmit = (data: UpdateLearningSetDto) => {
        if (!categoryId) {
            alert('Wybierz kategorię!');
            return;
        }
        createSetMutation.mutate(data);
    };

    return (
        <div className="container mx-auto p-4">
            <h1 className="text-2xl font-bold mb-4">Stwórz nowy zestaw</h1>

            <div className="mb-4">
                <label className="block mb-2">Kategoria</label>
                <select
                    value={categoryId}
                    onChange={e => setCategoryId(e.target.value)}
                    className="input-field"
                    required
                >
                    <option value="">-- Wybierz kategorię --</option>
                    {categories?.map(cat => (
                        <option key={cat.id} value={cat.id}>{cat.name}</option>
                    ))}
                </select>
            </div>

            <SetEditor
                onSubmit={handleSubmit}
                isSubmitting={createSetMutation.isPending}
            />
        </div>
    );
};

export default CreateSetPage;
